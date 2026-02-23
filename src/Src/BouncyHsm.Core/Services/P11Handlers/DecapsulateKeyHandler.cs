using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Encapsulators;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DecapsulateKeyHandler : IRpcRequestHandler<DecapsulateKeyRequest, DecapsulateKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<DecapsulateKeyHandler> logger;

    public DecapsulateKeyHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<DecapsulateKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async Task<DecapsulateKeyEnvelope> Handle(DecapsulateKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
           request.SessionId,
           (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);
        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "DecapsulateKey requires readwrite session");
        }

        PrivateKeyObject privateKeyObject = await this.hwServices.FindObjectByHandle<PrivateKeyObject>(memorySession, p11Session, request.PrivateKeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_DECAPSULATE);

        Dictionary<CKA, IAttributeValue> template = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        this.logger.LogTrace("Entering to CreateEncapsulator with mechanism type {mechanismType}", (CKM)request.Mechanism.MechanismType);
        P11EncapsulatorFactory p11EncapsulatorFactory = new P11EncapsulatorFactory(this.loggerFactory);
        IP11Encapsulator encapsulator = p11EncapsulatorFactory.Create(request.Mechanism, privateKeyObject);
        encapsulator.Init(template);

        SecretKeyObject secretKeyObject = encapsulator.Decapsulate(privateKeyObject, request.Ciphertext);
        secretKeyObject.Validate();
        uint phKeyHandle = await this.hwServices.StoreObject(memorySession, p11Session, secretKeyObject, cancellationToken);

        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation("Create new symmetric key using {generator}. Key <Id: {publicKeyId}, CK_ID: {publicKeyCkId}, CK_LABEL: {publicKeyCkLabel}>",
                encapsulator.ToString(),
                secretKeyObject.Id,
                Convert.ToHexString(secretKeyObject.CkaId),
                secretKeyObject.CkaLabel);
        }

        return new DecapsulateKeyEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new DecapsulateKeyDadta()
            {
                PhKeyHandle = phKeyHandle
            }
        };
    }
}