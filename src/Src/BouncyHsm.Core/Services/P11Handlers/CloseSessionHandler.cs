using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class CloseSessionHandler : IRpcRequestHandler<CloseSessionRequest, CloseSessionEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<CloseSessionHandler> logger;

    public CloseSessionHandler(IP11HwServices hwServices, ILogger<CloseSessionHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<CloseSessionEnvelope> Handle(CloseSessionRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with SessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);

        CloseSessionEnvelope envelope = new CloseSessionEnvelope();
        if (memorySession.DestroySession(request.SessionId))
        {
            envelope.Rv = (uint)CKR.CKR_OK;
        }
        else
        {
            envelope.Rv = (uint)CKR.CKR_SESSION_HANDLE_INVALID;
        }

        return envelope;
    }
}
