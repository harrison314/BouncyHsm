using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.UseCases.Implementation.SlotCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class InitTokenHandler : IRpcRequestHandler<InitTokenRequest, InitTokenEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly IProtectedAuthPathProvider protectedAuthPathProvider;
    private readonly ILogger<InitTokenHandler> logger;

    public InitTokenHandler(IP11HwServices hwServices, IProtectedAuthPathProvider protectedAuthPathProvider, ILogger<InitTokenHandler> logger)
    {
        this.hwServices = hwServices;
        this.protectedAuthPathProvider = protectedAuthPathProvider;
        this.logger = logger;
    }

    public async ValueTask<InitTokenEnvelope> Handle(InitTokenRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to  Handle with SlotId {SlotId}, pin length {pinLength} pin is set {pinIsSet}.",
            request.SlotId,
            request.Pin?.Length ?? 0,
            request.Pin != null);

        if (string.IsNullOrWhiteSpace(request.Label))
        {
            this.logger.LogError("Label cannot be null or empty for InitToken request.");
            throw new RpcPkcs11Exception(CKR.CKR_ARGUMENTS_BAD, "Label cannot be null or empty for InitToken request.");
        }

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(request.SlotId, true, cancellationToken);

        bool isLogged = await this.ExecuteLogin(request, slot, cancellationToken);
        if (!isLogged)
        {
            this.logger.LogError("Login failed for InitToken slot {SlotId}.", request.SlotId);
            return new InitTokenEnvelope()
            {
                Rv = (uint)CKR.CKR_PIN_INCORRECT
            };
        }

        await this.DestroyAllObjects(request.SlotId, cancellationToken);
        this.logger.LogInformation("All objects destroyed in slot {SlotId} for InitToken request.", request.SlotId);

        await this.hwServices.Persistence.ExecuteSlotCommand(request.SlotId,
            new ChangeLabelCommand(request.Label),
             cancellationToken);
        this.logger.LogInformation("Token label changed to {Label} in slot {SlotId} for InitToken request.", request.Label, request.SlotId);

        return new InitTokenEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    private async Task DestroyAllObjects(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DestroyAllObjects with SlotId {SlotId}.", slotId);

        Dictionary<CKA, IAttributeValue> filter = new Dictionary<CKA, IAttributeValue>()
        {
            { CKA.CKA_TOKEN, AttributeValue.Create(true) }
        };

        IReadOnlyList<StorageObject> objects = await this.hwServices.Persistence.FindObjects(slotId,
            new FindObjectSpecification(filter, true),
            cancellationToken);

        foreach (StorageObject storageObject in objects)
        {
            await this.hwServices.Persistence.DestroyObject(slotId,
                storageObject,
                cancellationToken);
        }
    }

    private async Task<bool> ExecuteLogin(InitTokenRequest request, SlotEntity slot, CancellationToken cancellationToken)
    {
        byte[]? utf8Pin = request.Pin;
        if (utf8Pin == null)
        {
            if (slot.Token.SimulateProtectedAuthPath)
            {
                utf8Pin = await this.protectedAuthPathProvider.TryLoginProtected(ProtectedAuthPathWindowType.Login, //TODO: seprate login type
                    CKU.CKU_SO,
                    slot,
                    cancellationToken);

                if (utf8Pin == null)
                {
                    this.logger.LogError("Login canceled using protected authorization path.");
                    throw new RpcPkcs11Exception(CKR.CKR_FUNCTION_CANCELED, "Login canceled using protected authorization path.");
                }
            }
            else
            {
                this.logger.LogError("Protected authorization path is not enabled in slot id {slotId} and token {tokenLabel}.",
                    slot.SlotId,
                    slot.Token.Label);
                throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, $"Protected authorization path is not enabled in slot id {slot.SlotId} and token {slot.Token.Label}.");
            }
        }

        bool pinIsValid = await this.hwServices.Persistence.ValidatePin(slot,
            CKU.CKU_SO,
            Encoding.UTF8.GetString(utf8Pin),
            null,
            cancellationToken);

        if (!pinIsValid)
        {
            this.logger.LogError("Invalid PIN for InitToken slot {SlotId}.",
                        request.SlotId);
        }

        return pinIsValid;
    }
}
