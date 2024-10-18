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
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        p11Session.State.EnsureEmpty();

        KeyObject objectInstance = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyObjectHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_DECRYPT);
        BufferedCipherWrapperFactory cipherFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        IBufferedCipherWrapper cipherWrapper = cipherFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IBufferedCipher bufferedCipher = cipherWrapper.IntoDecryption(objectInstance);

        p11Session.State = new DecryptState(bufferedCipher, (CKM)request.Mechanism.MechanismType);

        return new DecryptInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
