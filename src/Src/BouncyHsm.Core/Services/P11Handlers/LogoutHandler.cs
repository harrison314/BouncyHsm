using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class LogoutHandler : IRpcRequestHandler<LogoutRequest, LogoutEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<LogoutHandler> logger;

    public LogoutHandler(IP11HwServices hwServices, ILogger<LogoutHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public ValueTask<LogoutEnvelope> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to handle with SessionId {SessionId}.", request.SessionId);

        IP11Session session = this.hwServices.ClientAppCtx.EnsureSession(request.AppId, request.SessionId);

        CKR rv = CKR.CKR_USER_NOT_LOGGED_IN;

        if (session.IsLogged(CKU.CKU_USER))
        {
            session.SetLoginStatus(CKU.CKU_USER, false);
            this.logger.LogInformation("Logout user.");

            session.ClearState();

            rv = CKR.CKR_OK;
        }

        if (session.IsLogged(CKU.CKU_SO))
        {
            session.SetLoginStatus(CKU.CKU_SO, false);
            this.logger.LogInformation("Logout so.");

            session.ClearState();

            rv = CKR.CKR_OK;
        }

        return new ValueTask<LogoutEnvelope>(new LogoutEnvelope()
        {
            Rv = (uint)rv
        });
    }
}
