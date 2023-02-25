using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class VerifyInitHandler : IRpcRequestHandler<VerifyInitRequest, VerifyInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<VerifyInitHandler> logger;

    public VerifyInitHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<VerifyInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<VerifyInitEnvelope> Handle(VerifyInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
           request.SessionId,
           (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        p11Session.State.EnsureEmpty();

        KeyObject objectInstance = await hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyObjectHandle, cancellationToken);

        WrapperSignerFactory signerFactory = new WrapperSignerFactory(this.loggerFactory);

        IWrapperSigner signerWrapper = signerFactory.CreateSignatureAlgorithm(request.Mechanism);
        ISigner signer = signerWrapper.IntoValidationSigner(objectInstance);

        p11Session.State = new VerifyState(signer);

        return new VerifyInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
