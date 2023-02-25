using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class FinalizeHandler : IRpcRequestHandler<FinalizeRequest, FinalizeEnvelope>
{
    private readonly IClientApplicationContext clientApplicationContext;
    private readonly ILogger<FinalizeHandler> logger;

    public FinalizeHandler(IClientApplicationContext clientApplicationContext, ILogger<FinalizeHandler> logger)
    {
        this.clientApplicationContext = clientApplicationContext;
        this.logger = logger;
    }

    public ValueTask<FinalizeEnvelope> Handle(FinalizeRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req IsPtrSet {IsPtrSet}.",
            request.IsPtrSet);

        string key = DataTransform.GetApplicationKey(request.AppId);
        this.clientApplicationContext.ReleaseMemorySession(key);

        FinalizeEnvelope envelope = new FinalizeEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };

        return new ValueTask<FinalizeEnvelope>(envelope);
    }
}