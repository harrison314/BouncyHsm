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

    public ValueTask<GetSessionInfoEnvelope> Handle(GetSessionInfoRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with SessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session session = memorySession.EnsureSession(request.SessionId);

        uint flags = CKF.CKF_SERIAL_SESSION;
        if (session.IsRwSession)
        {
            flags |= CKF.CKF_RW_SESSION;
        }

        bool isLoggedToSlot = memorySession.IsUserLogged(session.SlotId);
        bool isSoLogin = session.IsLogged(CKU.CKU_SO);
        CKS sessionState = (isLoggedToSlot, isSoLogin, session.IsRwSession) switch
        {
            (false, false, _) => CKS.CKS_RO_PUBLIC_SESSION,
            (true, false, false) => CKS.CKS_RO_USER_FUNCTIONS,
            (true, false, true) => CKS.CKS_RW_USER_FUNCTIONS,
            (_, true, true) => CKS.CKS_RW_SO_FUNCTIONS,
            (_, true, false) => CKS.CKS_RW_SO_FUNCTIONS,
        };

        GetSessionInfoEnvelope envelope = new GetSessionInfoEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new SessionInfoData()
            {
                SlotId = session.SlotId,
                Flags = flags,
                State = (uint)sessionState,
                DeviceError = 0
            }
        };

        return new ValueTask<GetSessionInfoEnvelope>(envelope);
    }
}