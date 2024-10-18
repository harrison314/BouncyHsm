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

    public async ValueTask<LogoutEnvelope> Handle(LogoutRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to handle with SessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        CKR rv = CKR.CKR_USER_NOT_LOGGED_IN;

        if (p11Session.IsLogged(CKU.CKU_USER))
        {
            p11Session.SetLoginStatus(CKU.CKU_USER, false);
            this.logger.LogInformation("Logout user.");

            p11Session.ClearState();

            rv = CKR.CKR_OK;
        }

        if (p11Session.IsLogged(CKU.CKU_SO))
        {
            p11Session.SetLoginStatus(CKU.CKU_SO, false);
            this.logger.LogInformation("Logout so.");

            p11Session.ClearState();

            rv = CKR.CKR_OK;
        }

        return new LogoutEnvelope()
        {
            Rv = (uint)rv
        };
    }
}
