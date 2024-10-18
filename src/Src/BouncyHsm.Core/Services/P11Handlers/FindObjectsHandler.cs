using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class FindObjectsHandler : IRpcRequestHandler<FindObjectsRequest, FindObjectsEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<FindObjectsHandler> logger;

    public FindObjectsHandler(IP11HwServices hwServices, ILogger<FindObjectsHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<FindObjectsEnvelope> Handle(FindObjectsRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        FindObjectsState state = p11Session.State.Ensure<FindObjectsState>();

        uint[] handles = state.PullObjects(request.MaxObjectCount);

        return new FindObjectsEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new FindObjectsData()
            {
                Objects = handles,
                PullObjectCount = (uint)handles.Length
            }
        };
    }
}
