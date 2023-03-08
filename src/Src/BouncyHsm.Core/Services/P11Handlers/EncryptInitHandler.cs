using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class EncryptInitHandler : IRpcRequestHandler<EncryptInitRequest, EncryptInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<EncryptInitHandler> logger;

    public EncryptInitHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<EncryptInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<EncryptInitEnvelope> Handle(EncryptInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        p11Session.State.EnsureEmpty();

        KeyObject objectInstance = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyObjectHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_ENCRYPT);
        BufferedCipherWrapperFactory chiperFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        IBufferedCipherWrapper chiperWrapper = chiperFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IBufferedCipher bufferedChiper = chiperWrapper.IntoEncryption(objectInstance);

        p11Session.State = new EncryptState(bufferedChiper, (CKM)request.Mechanism.MechanismType);

        return new EncryptInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
