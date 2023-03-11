using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class UnwrapKeyHandler : IRpcRequestHandler<UnwrapKeyRequest, UnwrapKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<UnwrapKeyHandler> logger;

    public UnwrapKeyHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<UnwrapKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<UnwrapKeyEnvelope> Handle(UnwrapKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        KeyObject wrappingKey = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.UnwrappingKeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_UNWRAP);

        BufferedCipherWrapperFactory chiperFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        IBufferedCipherWrapper chiperWrapper = chiperFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IWrapper unwrapper = chiperWrapper.IntoUnwraping(wrappingKey);

        Dictionary<CKA, IAttributeValue> template = AttrTypeUtils.BuildDictionaryTemplate(request.Template);
        StorageObject storageObject = StorageObjectFactory.CreateEmpty(template);
        foreach ((CKA attrType, IAttributeValue attrValue) in template)
        {
            storageObject.SetValue(attrType, attrValue);
        }

        byte[] unwrapedKey = unwrapper.Unwrap(request.WrappedKeyData, 0, request.WrappedKeyData.Length);
        this.SetKeyValues(storageObject, unwrapedKey);

        storageObject.ReComputeAttributes();
        storageObject.Validate();

        uint handle = await this.hwServices.StoreObject(memorySession,
           p11Session,
           storageObject,
           cancellationToken);

        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation("Unwrap new key {keyName}. Key <Id: {keyId}, CK_LABEL: {publicKeyCkLabel}>",
                storageObject,
                storageObject.Id,
                storageObject.CkaLabel);
        }

        return new UnwrapKeyEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new UnwrapKeyData()
            {
                KeyHandle = handle
            }
        };
    }

    private void SetKeyValues(StorageObject storageObject, byte[] unwrapedKey)
    {
        this.logger.LogTrace("Entering to SetKeyValues.");

        if (storageObject is SecretKeyObject secretKeyObject)
        {
            secretKeyObject.SetSecret(unwrapedKey);
            secretKeyObject.CkaLocal = false;
            secretKeyObject.CkaNewerExtractable = false;
            secretKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwraped secret {secret}.", secretKeyObject);
        }
        else if (storageObject is RsaPrivateKeyObject privateKeyObject)
        {
            PrivateKeyInfo pki = new PrivateKeyInfo(new Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption),
                Asn1Object.FromByteArray(unwrapedKey));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymetricParams = PrivateKeyFactory.CreateKey(pki);
            privateKeyObject.SetPrivateKey(asymetricParams);

            privateKeyObject.CkaLocal = false;
            privateKeyObject.CkaNewerExtractable = false;
            privateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwraped private key {privateKey}.", privateKeyObject);
        }
        else if (storageObject is EcdsaPrivateKeyObject ecPrivateKeyObject)
        {
            DerObjectIdentifier curveNameOid = EcdsaUtils.ParseEcParamsOid(ecPrivateKeyObject.CkaEcParams);
            PrivateKeyInfo pki = new PrivateKeyInfo(new Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier(X9ObjectIdentifiers.IdECPublicKey, curveNameOid),
                Asn1Object.FromByteArray(unwrapedKey));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymetricParams = PrivateKeyFactory.CreateKey(pki);
            ecPrivateKeyObject.SetPrivateKey(asymetricParams);

            ecPrivateKeyObject.CkaLocal = false;
            ecPrivateKeyObject.CkaNewerExtractable = false;
            ecPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwraped private key {privateKey}.", ecPrivateKeyObject);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_UNWRAPPING_KEY_TYPE_INCONSISTENT,
                $"Can not crate key with invalid type {storageObject.GetType().Name}.");
        }
    }
}