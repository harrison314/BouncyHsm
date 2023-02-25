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
    private readonly ILogger<LoginHandler> logger;

    public LoginHandler(IP11HwServices hwServices, ILogger<LoginHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<LoginEnvelope> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to  Handle with sessionId {SessionId}, type {UserType}, pin length {pinLength} pin is set {pinIsSet}.",
            request.SessionId,
            request.UserType,
            request.Utf8Pin?.Length ?? 0,
            request.Utf8Pin != null);

        IP11Session session = this.hwServices.ClientAppCtx.EnsureSession(request.AppId, request.SessionId);
        Contracts.Entities.SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(session.SlotId, cancellationToken);

        if (slot.Token == null)
        {
            this.logger.LogError("Slot {SlotId} can not contains token", session.SlotId);
            return new LoginEnvelope()
            {
                Rv = (uint)CKR.CKR_TOKEN_NOT_PRESENT
            };
        }

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
            ISessionStateWithAlwaysAuthenticated state = session.State.Ensure<ISessionStateWithAlwaysAuthenticated>();
            if (!state.RequireContextPin)
            {
                this.logger.LogDebug("State not required CKU_CONTEXT_SPECIFIC login in session {SessionId}.", request.SessionId);
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_USER_ALREADY_LOGGED_IN
                };
            }

            if(state.IsContextPinHasSet)
            {
                this.logger.LogDebug("User already logged in specific context in session {SessionId}.", request.SessionId);
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_USER_ALREADY_LOGGED_IN
                };
            }

            bool pinIsValid = await this.ExecuteLogin(request, slot, session, cancellationToken);
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
            if (session.IsLogged(userType))
            {
                this.logger.LogDebug("User already logged in sessionId {SessionId}.", request.SessionId);
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_USER_ALREADY_LOGGED_IN
                };
            }

            bool pinIsValid = await this.ExecuteLogin(request, slot, session, cancellationToken);
            if (!pinIsValid)
            {
                return new LoginEnvelope()
                {
                    Rv = (uint)CKR.CKR_PIN_INCORRECT
                };
            }

            session.SetLoginStatus(userType, true);

            this.logger.LogInformation("Successfully logged to slot {slotId} with token {TokenSerial}.", slot.SlotId, slot.Token.SerialNumber);
            return new LoginEnvelope()
            {
                Rv = (uint)CKR.CKR_OK
            };
        }
    }

    private async Task<bool> ExecuteLogin(LoginRequest request, SlotEntity slot, IP11Session session, CancellationToken cancellationToken)
    {
        if (request.Utf8Pin == null)
        {
            //TODO: implement using eg. SignalR
            this.logger.LogError("Side channel authentication is not supported yet.");
            throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Side channel authentication is not supported yet.");
        }

        bool pinIsValid = await this.hwServices.Persistence.ValidatePin(slot,
            (CKU)request.UserType,
            Encoding.UTF8.GetString(request.Utf8Pin),
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
