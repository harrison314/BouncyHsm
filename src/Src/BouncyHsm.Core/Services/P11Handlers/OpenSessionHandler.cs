using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class OpenSessionHandler : IRpcRequestHandler<OpenSessionRequest, OpenSessionEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<OpenSessionHandler> logger;

    public OpenSessionHandler(IP11HwServices hwServices, ILogger<OpenSessionHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<OpenSessionEnvelope> Handle(OpenSessionRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Trace with SlotId {SlotId}, Flags {Flags}, IsPtrApplicationSet {IsPtrApplicationSet}, IsNotifySet {IsNotifySet}.",
            request.SlotId,
            request.Flags,
            request.IsPtrApplicationSet,
            request.IsNotifySet);

        Contracts.Entities.SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(request.SlotId, cancellationToken);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);

        bool isSerialSession = (request.Flags & CKF.CKF_SERIAL_SESSION) == CKF.CKF_SERIAL_SESSION;
        bool isRwSession = (request.Flags & CKF.CKF_RW_SESSION) == CKF.CKF_RW_SESSION;

        if (!isSerialSession)
        {
            return new OpenSessionEnvelope()
            {
                Rv = (uint)CKR.CKR_SESSION_PARALLEL_NOT_SUPPORTED,
                SessionId = 0
            };
        }

        SecureRandom random = (slot.Token.SimulateHwRng)
            ? BouncyHsm.Core.Services.Bc.HwRandomGenerator.SecureRandom
            : new SecureRandom();

        uint sessionId = memorySession.CreateSession(request.SlotId, isRwSession, random);

        return new OpenSessionEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            SessionId = sessionId
        };
    }
}
