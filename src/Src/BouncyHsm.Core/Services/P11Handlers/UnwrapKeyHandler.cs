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

        KeyObject unwrappingKey = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession, p11Session, request.UnwrappingKeyHandle, cancellationToken);

        MechanismUtils.CheckMechanism(request.Mechanism, MechanismCkf.CKF_UNWRAP);
        unwrappingKey.CheckAllowedMechanism((CKM)request.Mechanism.MechanismType, this.logger);

        BufferedCipherWrapperFactory cipherFactory = new BufferedCipherWrapperFactory(this.loggerFactory);
        ICipherWrapper cipherWrapper = cipherFactory.CreateCipherAlgorithm(request.Mechanism);
        Org.BouncyCastle.Crypto.IWrapper unwrapper = cipherWrapper.IntoUnwrapping(unwrappingKey);

        IReadOnlyDictionary<CKA, IAttributeValue> template = AttrTypeUtils.BuildDictionaryTemplate(request.Template);
        IReadOnlyDictionary<CKA, IAttributeValue> extendedKeyTemplate = this.ApplyUnwrapTemplate(template, unwrappingKey);

        StorageObject storageObject = StorageObjectFactory.CreateEmpty(extendedKeyTemplate);
        foreach ((CKA attrType, IAttributeValue attrValue) in extendedKeyTemplate)
        {
            storageObject.SetValue(attrType, attrValue, false);
        }

        byte[] unwrappedKey = unwrapper.Unwrap(request.WrappedKeyData, 0, request.WrappedKeyData.Length);
        this.SetKeyValues(storageObject, unwrappedKey, (CKM)request.Mechanism.MechanismType, extendedKeyTemplate);

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

    private IReadOnlyDictionary<CKA, IAttributeValue> ApplyUnwrapTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template, KeyObject unwrappingKey)
    {
        this.logger.LogTrace("Entering to ApplyUnwrapTemplate.");

        return unwrappingKey switch
        {
            SecretKeyObject secretKeyObject => AttrObjectTemplateUtils.MergeTemplates(unwrappingKey,
                secretKeyObject.CkaUnwrapTemplate,
                template,
                this.logger),
            PrivateKeyObject privateKeyObject => AttrObjectTemplateUtils.MergeTemplates(unwrappingKey,
               privateKeyObject.CkaUnwrapTemplate,
               template,
               this.logger),

            _ => template
        };
    }

    private void SetKeyValues(StorageObject storageObject, byte[] unwrappedKey, CKM mechanism, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to SetKeyValues.");

        bool useExplicitPading = MechanismUtils.IsUnwrapMechanismWithExplicitPading(mechanism);

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
        else if (storageObject is PrivateKeyObject privateKeyObject)
        {
            this.logger.LogTrace("Unwpraping {KeyType} private key", privateKeyObject.CkaKeyType);

            PrivateKeyInfo pki = PrivateKeyInfo.GetInstance(Asn1ObjectParser.FromByteArray(unwrappedKey, accetExtraData: useExplicitPading));

            Org.BouncyCastle.Crypto.AsymmetricKeyParameter asymmetricParams = PrivateKeyFactory.CreateKey(pki);
            privateKeyObject.SetPrivateKey(asymmetricParams);

            privateKeyObject.CkaLocal = false;
            privateKeyObject.CkaNewerExtractable = false;
            privateKeyObject.CkaAlwaysSensitive = false;

            this.logger.LogDebug("Unwrapped private key {privateKey}.", privateKeyObject);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_UNWRAPPING_KEY_TYPE_INCONSISTENT,
                $"Can not crate key with invalid type {storageObject.GetType().Name}.");
        }
    }

    private byte[] PadSecretKeyByTemplate(byte[] unwrappedKey, CKM mechanismType, IReadOnlyDictionary<CKA, IAttributeValue> template)
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