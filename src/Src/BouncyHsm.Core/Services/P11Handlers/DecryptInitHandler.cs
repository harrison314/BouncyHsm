using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DecryptInitHandler : IRpcRequestHandler<DecryptInitRequest, DecryptInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<DecryptInitHandler> logger;

    public DecryptInitHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<DecryptInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<DecryptInitEnvelope> Handle(DecryptInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
           request.SessionId,
           (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        p11Session.State.EnsureEmpty();

        KeyObject objectInstance = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyObjectHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_DECRYPT);
        BufferedCipherWrapperFactory chiperFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        IBufferedCipherWrapper chiperWrapper = chiperFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IBufferedCipher bufferedChiper = chiperWrapper.IntoDecryption(objectInstance);

        p11Session.State = new DecryptState(bufferedChiper, (CKM)request.Mechanism.MechanismType);

        return new DecryptInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
