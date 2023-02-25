using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class CloseAllSessionsHandler : IRpcRequestHandler<CloseAllSessionsRequest, CloseAllSessionsEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<CloseAllSessionsHandler> logger;

    public CloseAllSessionsHandler(IP11HwServices hwServices, ILogger<CloseAllSessionsHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<CloseAllSessionsEnvelope> Handle(CloseAllSessionsRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with SlotId {SlotId}.", request.SlotId);

        _ = await this.hwServices.Persistence.EnsureSlot(request.SlotId, cancellationToken);
        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);

        int count = memorySession.DestroySessionsBySlot(request.SlotId);
        this.logger.LogDebug("Destroy {0} sessions.", count);

        return new CloseAllSessionsEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}