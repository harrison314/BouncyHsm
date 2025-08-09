using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class PingHandler : IRpcRequestHandler<PingRequest, PingEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<SeedRandomHandler> logger;

    public PingHandler(IP11HwServices hwServices, ILogger<SeedRandomHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public Task<PingEnvelope> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle.");

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);

        //TODO: update TTL for memorySession instance

        this.logger.LogDebug("Update TTL for memory session.");

        return Task.FromResult(new PingEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        });
    }
}
