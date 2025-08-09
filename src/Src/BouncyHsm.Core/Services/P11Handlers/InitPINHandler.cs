using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class InitPINHandler : IRpcRequestHandler<InitPinRequest, InitPinEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly IProtectedAuthPathProvider protectedAuthPathProvider;
    private readonly ILogger<InitPINHandler> logger;

    public InitPINHandler(IP11HwServices hwServices,
        IProtectedAuthPathProvider protectedAuthPathProvider,
        ILogger<InitPINHandler> logger)
    {
        this.hwServices = hwServices;
        this.protectedAuthPathProvider = protectedAuthPathProvider;
        this.logger = logger;
    }

    public async Task<InitPinEnvelope> Handle(InitPinRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to  Handle with sessionId {SessionId}, pin length {pinLength} pin is set {pinIsSet}.",
           request.SessionId,
           request.Pin?.Length ?? 0,
           request.Pin != null);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);
        if (!p11Session.IsRwSession)
        {
            this.logger.LogError("Session {SessionId} is not a read-write session.", request.SessionId);
            return new InitPinEnvelope()
            {
                Rv = (uint)CKR.CKR_USER_NOT_LOGGED_IN
            };
        }

        SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(p11Session.SlotId, true, cancellationToken);

        byte[] pin = await this.GetPin(request, slot, cancellationToken);
        string pinStr = Encoding.UTF8.GetString(pin);
        await this.hwServices.Persistence.SetPin(slot, CKU.CKU_USER, pinStr, null, cancellationToken);

        this.logger.LogInformation("PIN initialized for slot {SlotId} with session {SessionId}.", slot.SlotId, request.SessionId);

        return new InitPinEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    private async Task<byte[]> GetPin(InitPinRequest request, SlotEntity slot, CancellationToken cancellationToken)
    {
        byte[]? utf8Pin = request.Pin;
        if (utf8Pin == null)
        {
            if (slot.Token.SimulateProtectedAuthPath)
            {
                utf8Pin = await this.protectedAuthPathProvider.TryLoginProtected(ProtectedAuthPathWindowType.InitPin,
                    CKU.CKU_USER,
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

        return utf8Pin;
    }
}