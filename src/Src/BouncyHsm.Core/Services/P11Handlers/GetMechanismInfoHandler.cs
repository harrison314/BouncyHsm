using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetMechanismInfoHandler : IRpcRequestHandler<GetMechanismInfoRequest, GetMechanismInfoEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetMechanismInfoHandler> logger;

    public GetMechanismInfoHandler(IP11HwServices hwServices, ILogger<GetMechanismInfoHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<GetMechanismInfoEnvelope> Handle(GetMechanismInfoRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req SlotId {SlotId} and mechanism type {MechanismType}.",
           request.SlotId,
           request.MechanismType);

        _ = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        Contracts.Entities.SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(request.SlotId, true, cancellationToken);

        if (MechanismUtils.TryGetMechanismInfo(request.MechanismType, out Common.MechanismInfo mechanismInfo))
        {
            return new GetMechanismInfoEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new Rpc.MechanismInfo()
                {
                    MechanismType = request.MechanismType,
                    MinKeySize = mechanismInfo.MinKeySize,
                    MaxKeySize = mechanismInfo.MaxKeySize,
                    Flags = (uint)((slot.Token.SimulateHwMechanism) ? mechanismInfo.Flags : mechanismInfo.Flags | MechanismCkf.CKF_HW)
                }
            };
        }
        else
        {
            return new GetMechanismInfoEnvelope()
            {
                Rv = (uint)CKR.CKR_MECHANISM_INVALID,
                Data = null
            };
        }
    }
}