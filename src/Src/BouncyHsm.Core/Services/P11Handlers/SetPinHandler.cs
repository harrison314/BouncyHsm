using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SetPinHandler : IRpcRequestHandler<SetPinRequest, SetPinEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly IProtectedAuthPathProvider protectedAuthPathProvider;
    private readonly ILogger<SetPinHandler> logger;

    public SetPinHandler(IP11HwServices hwServices, IProtectedAuthPathProvider protectedAuthPathProvider, ILogger<SetPinHandler> logger)
    {
        this.hwServices = hwServices;
        this.protectedAuthPathProvider = protectedAuthPathProvider;
        this.logger = logger;
    }

    public async ValueTask<SetPinEnvelope> Handle(SetPinRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to  Handle with sessionId {SessionId}, old pin length {oldPinLength}, old pin is set {oldPinIsSet}, new pin length {newPinLength}, new pin is set {newPinIsSet}.",
            request.SessionId,
            request.Utf8OldPin?.Length ?? 0,
            request.Utf8OldPin != null,
            request.Utf8NewPin?.Length ?? 0,
            request.Utf8NewPin != null);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);
        SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(p11Session.SlotId, true, cancellationToken);

        if (!p11Session.IsRwSession)
        {
            this.logger.LogError("Session {sessionId} must by read-write session for change PIN.", request.SessionId);
            return new SetPinEnvelope()
            {
                Rv = (uint)CKR.CKR_SESSION_READ_ONLY
            };
        }

        CKU userType = CKU.CKU_USER;
        if (p11Session.IsLogged(CKU.CKU_SO))
        {
            userType = CKU.CKU_SO;
        }

        string oldPin = await this.ObtrainPin(ProtectedAuthPathWindowType.Login,
            request.Utf8OldPin,
            userType,
            slot,
            p11Session,
            cancellationToken);

        bool oldPinIsValid = await this.hwServices.Persistence.ValidatePin(slot,
           userType,
           oldPin,
           null,
           cancellationToken);

        if (!oldPinIsValid)
        {
            this.logger.LogError("Invalid PIN in SetPIN for slot {SlotId} and type {PinType}.", slot.SlotId, userType);
            return new SetPinEnvelope()
            {
                Rv = (uint)CKR.CKR_PIN_INCORRECT
            };
        }

        string newPin = await this.ObtrainPin(ProtectedAuthPathWindowType.SetPin,
            request.Utf8NewPin,
            userType,
            slot,
            p11Session,
            cancellationToken);

        if (string.IsNullOrEmpty(newPin))
        {
            this.logger.LogError("New PIN for slot {SlotId} and type {PinType} is empty.", slot.SlotId, userType);
            return new SetPinEnvelope()
            {
                Rv = (uint)CKR.CKR_PIN_INVALID
            };
        }

        await this.hwServices.Persistence.SetPin(slot,
           userType,
           newPin,
           null,
           cancellationToken);

        this.logger.LogInformation("PIN changed for slot {SlotId} and type {PinType}.", slot.SlotId, userType);
        return new SetPinEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    private async Task<string> ObtrainPin(ProtectedAuthPathWindowType windowType, byte[]? utf8Pin, CKU userType, SlotEntity slot, IP11Session session, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ObtrainPin with windowType {windowType}, userType {userType}.", windowType, userType);
        
        if (utf8Pin == null)
        {
            if (slot.Token.SimulateProtectedAuthPath)
            {
                utf8Pin = await this.protectedAuthPathProvider.TryLoginProtected(windowType,
                    userType,
                    slot,
                    cancellationToken);

                if (utf8Pin == null)
                {
                    this.logger.LogError("PIN action canceled using protected authorization path.");
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

        return Encoding.UTF8.GetString(utf8Pin);
    }
}
