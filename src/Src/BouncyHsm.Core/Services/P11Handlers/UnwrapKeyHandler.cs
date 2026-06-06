using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.Bc;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1;
using Microsoft.Extensions.Logging;

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

    public async Task<UnwrapKeyEnvelope> Handle(UnwrapKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} mechanism {mechanism}.",
            request.SessionId,
            (CKM)request.Mechanism.MechanismType);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        KeyObject wrappingKey = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.UnwrappingKeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_UNWRAP);
        wrappingKey.CheckAllowedMechanism((CKM)request.Mechanism.MechanismType, this.logger);

        BufferedCipherWrapperFactory cipherFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        ICipherWrapper cipherWrapper = cipherFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IWrapper unwrapper = cipherWrapper.IntoUnwrapping(wrappingKey);

        Dictionary<CKA, IAttributeValue> template = AttrTypeUtils.BuildDictionaryTemplate(request.Template);
        StorageObject storageObject = StorageObjectFactory.CreateEmpty(template);
        foreach ((CKA attrType, IAttributeValue attrValue) in template)
        {
            storageObject.SetValue(attrType, attrValue, false);
        }

        byte[] unwrappedKey = unwrapper.Unwrap(request.WrappedKeyData, 0, request.WrappedKeyData.Length);
        this.SetKeyValues(storageObject, unwrappedKey, (CKM)request.Mechanism.MechanismType, template);

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

    private void SetKeyValues(StorageObject storageObject, byte[] unwrappedKey, CKM mechanism, Dictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to SetKeyValues.");

        bool useExplicitPading = MechanismUtils.IsUnwrapMechanismWithExplicitPading(mechanism);

        //TODO: refactor to visitor
        if (storageObject is SecretKeyObject secretKeyObject)
        {
            this.logger.LogTrace("Unwpraping secret of type {CkaKeyType}", secretKeyObject.CkaKeyType);

            if (useExplicitPading)
            {
                uint? requiredLength = secretKeyObject.GetRequiredSecretLen();
                if (requiredLength.HasValue)
                {
                    unwrappedKey = unwrappedKey[..((int)requiredLength.Value)];
                }
                else
                {
                    unwrappedKey = this.PadSecretKeyByTemplate(unwrappedKey, mechanism, template);
                }
            }

            secretKeyObject.SetSecret(unwrappedKey);
            secretKeyObject.CkaLocal = false;
            secretKeyObject.CkaNewerExtractable = false;
            secretKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped secret {secret}.", secretKeyObject);
        }
        else if (storageObject is RsaPrivateKeyObject privateKeyObject)
        {
            this.logger.LogTrace("Unwpraping RSA private key");

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            privateKeyObject.SetPrivateKey(asymmetricParams);

            privateKeyObject.CkaLocal = false;
            privateKeyObject.CkaNewerExtractable = false;
            privateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", privateKeyObject);
        }
        else if (storageObject is EcdsaPrivateKeyObject ecPrivateKeyObject)
        {
            this.logger.LogTrace("Unwpraping ECDSA private key");

            Asn1Object asn1EcParams = EcdsaUtils.ParseEcParamsToAsn1Object(ecPrivateKeyObject.CkaEcParams);

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            ecPrivateKeyObject.SetPrivateKey(asymmetricParams);

            ecPrivateKeyObject.CkaLocal = false;
            ecPrivateKeyObject.CkaNewerExtractable = false;
            ecPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", ecPrivateKeyObject);
        }
        else if (storageObject is MlDsaPrivateKeyObject mldsaPrivateKeyObject)
        {
            this.logger.LogTrace("Unwpraping ML-DSA private key");

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            mldsaPrivateKeyObject.SetPrivateKey(asymmetricParams);

            mldsaPrivateKeyObject.CkaLocal = false;
            mldsaPrivateKeyObject.CkaNewerExtractable = false;
            mldsaPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", mldsaPrivateKeyObject);
        }
        else if (storageObject is SlhDsaPrivateKeyObject slhdsaPrivateKeyObject)
        {
            this.logger.LogTrace("Unwpraping SLH-DSA private key");

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            slhdsaPrivateKeyObject.SetPrivateKey(asymmetricParams);

            slhdsaPrivateKeyObject.CkaLocal = false;
            slhdsaPrivateKeyObject.CkaNewerExtractable = false;
            slhdsaPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", slhdsaPrivateKeyObject);
        }
        else if (storageObject is MlKemPrivateKeyObject mlKemPrivateKeyObject)
        {
            this.logger.LogTrace("Unwpraping ML-KEM private key");

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            mlKemPrivateKeyObject.SetPrivateKey(asymmetricParams);

            mlKemPrivateKeyObject.CkaLocal = false;
            mlKemPrivateKeyObject.CkaNewerExtractable = false;
            mlKemPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", mlKemPrivateKeyObject);
        }
        else if (storageObject is EdwardsPrivateKeyObject edwardsPrivateKeyObject)
        {
            this.logger.LogTrace("Unwpraping Edwards private key");

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            edwardsPrivateKeyObject.SetPrivateKey(asymmetricParams);

            edwardsPrivateKeyObject.CkaLocal = false;
            edwardsPrivateKeyObject.CkaNewerExtractable = false;
            edwardsPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", edwardsPrivateKeyObject);
        }
        else if (storageObject is MontgomeryPrivateKeyObject montgomeryPrivateKeyObject)
        {
            this.logger.LogTrace("Unwpraping Montgomery private key");

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            montgomeryPrivateKeyObject.SetPrivateKey(asymmetricParams);

            montgomeryPrivateKeyObject.CkaLocal = false;
            montgomeryPrivateKeyObject.CkaNewerExtractable = false;
            montgomeryPrivateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", montgomeryPrivateKeyObject);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_UNWRAPPING_KEY_TYPE_INCONSISTENT,
                $"Can not crate key with invalid type {storageObject.GetType().Name}.");
        }
    }

    private byte[] PadSecretKeyByTemplate(byte[] unwrappedKey, CKM mechanismType, Dictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to PadSecretKeyByTemplate.");

        if (template.TryGetValue(CKA.CKA_VALUE_LEN, out IAttributeValue? attributeValue))
        {
            int prefedLength = (int)attributeValue.AsUint();
            if (unwrappedKey.Length < prefedLength)
            {
                this.logger.LogError("Invalid lenrth of CKA_VALUE_LEN ({actualValueLength}) but unwraped key has length {unwrapedKeyLen} for {mecyhnismType}.",
                    prefedLength,
                    unwrappedKey.Length,
                    mechanismType);
                throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                    $"Invalid lenrth of CKA_VALUE_LEN ({prefedLength}) but unwraped key has length {unwrappedKey.Length} for {mechanismType}.");
            }

            return unwrappedKey[..prefedLength];
        }
        else
        {
            this.logger.LogError("Unwrap with mechanism {mecyhnismType} required defined CKA_VALUE_LEN for secrets.", mechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
                $"Unwrap with mechanism {mechanismType} required defined CKA_VALUE_LEN for secrets.");
        }
    }
}