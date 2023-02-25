using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetInfoHandler : IRpcRequestHandler<GetInfoRequest, GetInfoEnvelope>
{
    private readonly ILogger<GetInfoHandler> logger;

    public GetInfoHandler(ILogger<GetInfoHandler> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<GetInfoEnvelope> Handle(GetInfoRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle.");

        await Task.Delay(0);

        return new GetInfoEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            ManufacturerID = P11Constants.ManufacturerId
        };
    }
}
