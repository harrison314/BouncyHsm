using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class FindObjectsFinalHandler : IRpcRequestHandler<FindObjectsFinalRequest, FindObjectsFinalEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<FindObjectsFinalHandler> logger;

    public FindObjectsFinalHandler(IP11HwServices hwServices, ILogger<FindObjectsFinalHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public ValueTask<FindObjectsFinalEnvelope> Handle(FindObjectsFinalRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        FindObjectsState state = p11Session.State.Ensure<FindObjectsState>();
        p11Session.ClearState();

        return new ValueTask<FindObjectsFinalEnvelope>(new FindObjectsFinalEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        });
    }
}