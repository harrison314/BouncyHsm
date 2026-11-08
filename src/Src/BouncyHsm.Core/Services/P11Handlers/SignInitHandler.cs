using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SignInitHandler : IRpcRequestHandler<SignInitRequest, SignInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<SignInitHandler> logger;

    public SignInitHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<SignInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async Task<SignInitEnvelope> Handle(SignInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_SIGN);

        p11Session.State.EnsureEmpty();

        KeyObject objectInstance = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.KeyObjectHandle, cancellationToken);
        objectInstance.CheckAllowedMechanism((CKM)request.Mechanism.MechanismType, this.logger);

        WrapperSignerFactory signerFactory = new WrapperSignerFactory(this.loggerFactory);

        IWrapperSigner signerWrapper = signerFactory.CreateSignatureAlgorithm(request.Mechanism);
        p11Session.State = this.CreateSessionState(p11Session, objectInstance, signerWrapper);

        return new SignInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    private ISessionState CreateSessionState(IP11Session p11Session, KeyObject objectInstance, IWrapperSigner signerWrapper)
    {
        if (signerWrapper is IStatefulWrapperSigner statefulSignerWrapper)
        {
            AuthenticatedStatefulSigner statefulSigner = statefulSignerWrapper.IntoStatefulSigner(objectInstance, this.hwServices, p11Session);

            return new SignStateWithStatefulSigner(statefulSigner.Signer,
                objectInstance.Id,
                statefulSigner.RequireSignaturePin,
                objectInstance.CkaPrivate);
        }
        else
        {
            AuthenticatedSigner signer = signerWrapper.IntoSigningSigner(objectInstance, p11Session.SecureRandom);

            return new SignStateWithSigner(signer.Signer,
                objectInstance.Id,
                signer.RequireSignaturePin,
                objectInstance.CkaPrivate,
                signerWrapper.AcceptEmptyData);
        }
    }
}
