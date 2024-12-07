using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SignRecoverInitHandler : IRpcRequestHandler<SignRecoverInitRequest, SignRecoverInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<SignRecoverInitHandler> logger;

    public SignRecoverInitHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<SignRecoverInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public  async ValueTask<SignRecoverInitEnvelope> Handle(SignRecoverInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_SIGN_RECOVER);
        p11Session.State.EnsureEmpty();

        KeyObject objectInstance = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyObjectHandle, cancellationToken);

        
        WrapperSignWithRecoverFactory signerFactory = new WrapperSignWithRecoverFactory(this.loggerFactory);

        IWrapperSignWithRecover signerWrapper = signerFactory.CreateSignatureWithAlgorithm(request.Mechanism);
        AuthenticatedSignerWithRecovery signer = signerWrapper.IntoSigningSigner(objectInstance, p11Session.SecureRandom);

        p11Session.State = new SignWithRecoverState(signer.Signer, objectInstance.Id, signer.RequireSignaturePin, objectInstance.CkaPrivate);

        return new SignRecoverInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}