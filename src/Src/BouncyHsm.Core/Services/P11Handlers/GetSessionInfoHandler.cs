using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetSessionInfoHandler : IRpcRequestHandler<GetSessionInfoRequest, GetSessionInfoEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetSessionInfoHandler> logger;

    public GetSessionInfoHandler(IP11HwServices hwServices, ILogger<GetSessionInfoHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<GetSessionInfoEnvelope> Handle(GetSessionInfoRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with SessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        uint flags = CKF.CKF_SERIAL_SESSION;
        if (p11Session.IsRwSession)
        {
            flags |= CKF.CKF_RW_SESSION;
        }

        bool isLoggedToSlot = memorySession.IsUserLogged(p11Session.SlotId);
        bool isSoLogin = p11Session.IsLogged(CKU.CKU_SO);
        CKS sessionState = (isLoggedToSlot, isSoLogin, p11Session.IsRwSession) switch
        {
            (false, false, _) => CKS.CKS_RO_PUBLIC_SESSION,
            (true, false, false) => CKS.CKS_RO_USER_FUNCTIONS,
            (true, false, true) => CKS.CKS_RW_USER_FUNCTIONS,
            (_, true, true) => CKS.CKS_RW_SO_FUNCTIONS,
            (_, true, false) => CKS.CKS_RW_SO_FUNCTIONS,
        };

        return new GetSessionInfoEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new SessionInfoData()
            {
                SlotId = p11Session.SlotId,
                Flags = flags,
                State = (uint)sessionState,
                DeviceError = 0
            }
        };
    }
}