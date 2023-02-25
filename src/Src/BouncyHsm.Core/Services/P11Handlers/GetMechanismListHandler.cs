using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetMechanismListHandler : IRpcRequestHandler<GetMechanismListRequest, GetMechanismListEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetMechanismListHandler> logger;

    public GetMechanismListHandler(IP11HwServices hwServices, ILogger<GetMechanismListHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<GetMechanismListEnvelope> Handle(GetMechanismListRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req SlotId {SlotId}.",
            request.SlotId);

        _ = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        _ = await this.hwServices.Persistence.EnsureSlot(request.SlotId, cancellationToken);

        uint[] mechanismTypes = MechanismUtils.GetMechanismAsUintArray();

        if (request.IsMechanismListPointerPresent)
        {
            if (request.PullCount < mechanismTypes.Length)
            {
                return new GetMechanismListEnvelope()
                {
                    Rv = (uint)CKR.CKR_BUFFER_TOO_SMALL,
                    Data = null
                };
            }
            else
            {
                return new GetMechanismListEnvelope()
                {
                    Rv = (uint)CKR.CKR_OK,
                    Data = new MechanismList()
                    {
                        MechanismTypes = mechanismTypes,
                        PullCount = (uint)mechanismTypes.Length
                    }
                };
            }
        }
        else
        {
            return new GetMechanismListEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new MechanismList()
                {
                    MechanismTypes = Array.Empty<uint>(),
                    PullCount = (uint)mechanismTypes.Length
                }
            };
        }
    }
}
