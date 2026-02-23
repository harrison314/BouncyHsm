using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using System.Security.Cryptography.X509Certificates;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal abstract class P11EncapsulatorBase<TPublicKey, TPrivateKey> : IP11Encapsulator
    where TPublicKey : PublicKeyObject
    where TPrivateKey : PrivateKeyObject
{
    protected readonly ILogger logger;
    protected readonly CKM mechynismType;
    private SecretKeyObject? secretKeyObject;
    private uint? requestedKeyLenusingAttribute;

    protected P11EncapsulatorBase(ILogger logger, CKM mechynismType)
    {
        this.logger = logger;
        this.mechynismType = mechynismType;
    }

    public void Init(Dictionary<CKA, IAttributeValue> template)
    {
        this.secretKeyObject = StorageObjectFactory.CreateSecret(template);
        foreach (KeyValuePair<CKA, IAttributeValue> kvp in template)
        {
            if (kvp.Key == CKA.CKA_CLASS || kvp.Key == CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            this.secretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        if (!template.TryGetValue(CKA.CKA_EXTRACTABLE, out IAttributeValue? attrValueLen))
        {
            this.secretKeyObject.CkaExtractable = true;
        }

        if (template.TryGetValue(CKA.CKA_VALUE_LEN, out IAttributeValue? valueLenAttr))
        {
            this.requestedKeyLenusingAttribute = valueLenAttr.AsUint();
        }
        else
        {
            this.requestedKeyLenusingAttribute = null;
        }
    }

    public EncapsulationResult Encapsulate(PublicKeyObject publicKey, SecureRandom secureRandom)
    {
        System.Diagnostics.Debug.Assert(this.secretKeyObject != null, "Init must be called first.");

        this.logger.LogTrace("Entering to Encapsulate.");
        if (publicKey is TPublicKey typedPublicKey)
        {
            if (!typedPublicKey.CkaEncapsulate)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_ENCAPSULATE to true.", typedPublicKey.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The encapsulate operation is not allowed because objet is not authorized to sign (CKA_ENCAPSULATE must by true).");
            }

            this.EncapsulateInternal(typedPublicKey, this.secretKeyObject, secureRandom, out byte[] encapsulatedData);
            this.UpdateSecretKeyAttributes(this.secretKeyObject);

            return new EncapsulationResult(this.secretKeyObject, encapsulatedData);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechynismType} required public key {typeof(TPublicKey).Name}.");
        }
    }

    protected abstract void EncapsulateInternal(TPublicKey publicKey, SecretKeyObject secretKeyObject, SecureRandom secureRandom, out byte[] encapsulatedData);

    public uint GetEncapsulatedDataLength(PublicKeyObject publicKey)
    {
        this.logger.LogTrace("Entering to GetEncapsulatedDataLength.");
        if (publicKey is TPublicKey typedPublicKey)
        {
            if (!typedPublicKey.CkaEncapsulate)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_ENCAPSULATE to true.", typedPublicKey.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The encapsulate operation is not allowed because objet is not authorized to sign (CKA_ENCAPSULATE must by true).");
            }

            return (uint)this.GetEncapsulatedDataLengthInternal(typedPublicKey);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechynismType} required public key {typeof(TPublicKey).Name}.");
        }
    }

    protected abstract int GetEncapsulatedDataLengthInternal(TPublicKey publicKey);

    public SecretKeyObject Decapsulate(PrivateKeyObject privateKey, byte[] encapsulatedData)
    {
        System.Diagnostics.Debug.Assert(this.secretKeyObject != null, "Init must be called first.");

        this.logger.LogTrace("Entering to Decapsulate.");
        if (privateKey is TPrivateKey typedPrivateKey)
        {
            if (!typedPrivateKey.CkaDecapsulate)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_DECAPSULATE to true.", typedPrivateKey.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The encapsulate operation is not allowed because objet is not authorized to sign (CKA_DECAPSULATE must by true).");
            }

            this.DecapsulateInternal(typedPrivateKey, encapsulatedData, this.secretKeyObject);
            this.UpdateSecretKeyAttributes(this.secretKeyObject);
            return this.secretKeyObject;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechynismType} required private key {typeof(TPublicKey).Name}.");
        }
    }

    protected abstract void DecapsulateInternal(TPrivateKey privateKey, byte[] encapsulatedData, SecretKeyObject secretKeyObject);

    private void UpdateSecretKeyAttributes(SecretKeyObject secretKeyObject)
    {
        secretKeyObject.CkaSensitive = false;
        secretKeyObject.CkaNewerExtractable = false;
        secretKeyObject.CkaLocal = false;
        secretKeyObject.ReComputeAttributes();
    }

    protected void SetSecretKeyPadded(SecretKeyObject secretKeyObject, byte[] secret)
    {
        this.logger.LogTrace("Entering to SetSecretKeyPadded.");

        uint? requiredSecretkeyLen = secretKeyObject.GetRequiredSecretLen();
        this.logger.LogTrace("Key type {keyType} required secret length {requiredSecretkeyLen}, template define secret length {templateSecretLength}.",
            secretKeyObject.CkaKeyType,
            requiredSecretkeyLen,
            this.requestedKeyLenusingAttribute);

        if (!requiredSecretkeyLen.HasValue)
        {
            requiredSecretkeyLen = this.requestedKeyLenusingAttribute;
        }

        if (requiredSecretkeyLen.HasValue)
        {
            if (secret.Length == requiredSecretkeyLen.Value)
            {
                secretKeyObject.SetSecret(secret);
                return;
            }

            if (secret.Length > requiredSecretkeyLen.Value)
            {
                byte[] truncatedSecret = new byte[requiredSecretkeyLen.Value];
                Array.Copy(secret, 0, truncatedSecret, 0, truncatedSecret.Length);
                secretKeyObject.SetSecret(truncatedSecret);

                this.logger.LogInformation("Derived secret key was truncated from {OriginalLen} to {TruncatedLen} for {KeyObject}.",
                    secret.Length,
                    truncatedSecret.Length,
                    secretKeyObject.GetType().Name);
                return;
            }

            throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
                $"The generated secret key length {secret.Length} is less than required length {requiredSecretkeyLen.Value} for key {secretKeyObject.GetType().Name} ({secretKeyObject.CkaKeyType}).");
        }

        uint minimalSecretLen = secretKeyObject.GetMinimalSecretLen();
        if (secret.Length < minimalSecretLen)
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
               $"The generated secret key length {secret.Length} is less than minmal length {minimalSecretLen} for key {secretKeyObject.GetType().Name} ({secretKeyObject.CkaKeyType}).");
        }

        secretKeyObject.SetSecret(secret);
    }

    protected int GetMinimalSecretLength()
    {
        System.Diagnostics.Debug.Assert(this.secretKeyObject != null, "Init must be called first.");
        uint? requiredSecretkeyLen = this.secretKeyObject.GetRequiredSecretLen();

        if (!requiredSecretkeyLen.HasValue)
        {
            requiredSecretkeyLen = this.requestedKeyLenusingAttribute;
        }

        if (!requiredSecretkeyLen.HasValue)
        {
            requiredSecretkeyLen = this.secretKeyObject.GetMinimalSecretLen();
        }

        return (int)requiredSecretkeyLen.Value;
    }
}