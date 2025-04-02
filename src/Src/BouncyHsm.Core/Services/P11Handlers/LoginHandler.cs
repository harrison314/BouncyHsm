using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class LoginHandler : IRpcRequestHandler<LoginRequest, LoginEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly IProtectedAuthPathProvider protectedAuthPathProvider;
    private readonly ILogger<LoginHandler> logger;

    public LoginHandler(IP11HwServices hwServices, IProtectedAuthPathProvider protectedAuthPathProvider, ILogger<LoginHandler> logger)
    {
        this.hwServices = hwServices;
        this.protectedAuthPathProvider = protectedAuthPathProvider;
        this.logger = logger;
    }

    public async ValueTask<LoginEnvelope> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to  Handle with sessionId {SessionId}, type {UserType}, pin length {pinLength} pin is set {pinIsSet}.",
            request.SessionId,
            (CKU)request.UserType,
            request.Utf8Pin?.Length ?? 0,
            request.Utf8Pin != null);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);
        SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(p11Session.SlotId, true, cancellationToken);

        CKU userType = (CKU)request.UserType;
        if (!Enum.IsDefined(userType))
        {
            this.logger.LogError("User type {userType} is not valid value.", request.UserType);
            return new LoginEnvelope()
            {
                Rv = (uint)CKR.CKR_ARGUMENTS_BAD
            };
        }

        if (userType == CKU.CKU_CONTEXT_SPECIFIC)
        {
            ISessionStateWithAlwaysAuthenticated state = p11Session.State.Ensure<ISessionStateWithAlwaysAuthenticated>();
            if (!state.RequireContextPin)
            {
                this.logger.LogDebug("State not required CKU_CONTEXT_SPECIFIC login in session {SessionId}.", request.SessionId);
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_USER_ALREADY_LOGGED_IN
                };
            }

            if (state.IsContextPinHasSet)
            {
                this.logger.LogDebug("User already logged in specific context in session {SessionId}.", request.SessionId);
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_USER_ALREADY_LOGGED_IN
                };
            }

            bool pinIsValid = await this.ExecuteLogin(request, slot, p11Session, cancellationToken);
            if (!pinIsValid)
            {
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_PIN_INCORRECT
                };
            }

            state.ContextLogin();

            this.logger.LogInformation("User logged in specific context in session {SessionId}.", request.SessionId);
            return new LoginEnvelope()
            {
                Rv = (uint)CKR.CKR_OK
            };
        }
        else
        {
            if (p11Session.IsLogged(userType))
            {
                this.logger.LogDebug("User already logged in sessionId {SessionId}.", request.SessionId);
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_USER_ALREADY_LOGGED_IN
                };
            }

            bool pinIsValid = await this.ExecuteLogin(request, slot, p11Session, cancellationToken);
            if (!pinIsValid)
            {
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_PIN_INCORRECT
                };
            }

            p11Session.SetLoginStatus(userType, true);

            this.logger.LogInformation("Successfully logged to slot {slotId} with token {TokenSerial}.", slot.SlotId, slot.Token.SerialNumber);
            return new LoginEnvelope()
            {
                Rv = (uint)CKR.CKR_OK
            };
        }
    }

    private async Task<bool> ExecuteLogin(LoginRequest request, SlotEntity slot, IP11Session session, CancellationToken cancellationToken)
    {
        byte[]? utf8Pin = request.Utf8Pin;
        if (utf8Pin == null)
        {
            if (slot.Token.SimulateProtectedAuthPath)
            {
                utf8Pin = await this.protectedAuthPathProvider.TryLoginProtected(ProtectedAuthPathWindowType.Login,
                    (CKU)request.UserType,
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
            (CKU)request.UserType,
            Encoding.UTF8.GetString(utf8Pin),
            null,
            cancellationToken);

        if (!pinIsValid)
        {
            this.logger.LogError("Invalid PIN for slot {SlotId} and type {PinType}.",
                        session.SlotId,
                        (CKU)request.UserType);
        }

        return pinIsValid;
    }
}
