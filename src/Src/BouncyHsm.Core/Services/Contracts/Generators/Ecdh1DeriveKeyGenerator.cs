using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class Ecdh1DeriveKeyGenerator : IDeriveKeyGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;
    private readonly Ecdh1DeriveParams ecdh1Params;
    private readonly ILogger<Ecdh1DeriveKeyGenerator> logger;

    public Ecdh1DeriveKeyGenerator(Ecdh1DeriveParams ecdh1Params, ILogger<Ecdh1DeriveKeyGenerator> logger)
    {
        this.ecdh1Params = ecdh1Params;
        this.logger = logger;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to init");

        this.CheckTemplate(template);

        this.template = template;
    }

    public SecretKeyObject Generate(StorageObject baseKey)
    {
        this.logger.LogTrace("Entering to Generate.");

        System.Diagnostics.Debug.Assert(this.template != null);

        if (baseKey is not EcdsaPrivateKeyObject && baseKey is not MontgomeryPrivateKeyObject)
        {
            this.logger.LogError("Base key handle is invalid  is {baseKey}. Excepted EcdsaPrivateKeyObject or MontgomeryPrivateKeyObject.", baseKey);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID,
              $"Base key handle is invalid, is not EcdsaPrivateKeyObject or MontgomeryPrivateKeyObject.");
        }

        PrivateKeyObject sBaseKey = (PrivateKeyObject)baseKey;

        SecretKeyObject generalSecretKeyObject = StorageObjectFactory.CreateSecret(this.template);
        generalSecretKeyObject.CkaSensitive = sBaseKey.CkaSensitive;
        generalSecretKeyObject.CkaExtractable = sBaseKey.CkaExtractable;

        foreach (KeyValuePair<CKA, IAttributeValue> kvp in this.template)
        {
            if (kvp.Key == CKA.CKA_CLASS)
            {
                continue;
            }

            generalSecretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        byte[] secret = sBaseKey switch
        {
            EcdsaPrivateKeyObject ecdsaPrivateKey => this.DeriveSecret(generalSecretKeyObject, ecdsaPrivateKey, this.template),
            MontgomeryPrivateKeyObject mongomeryPrivateKey => this.DeriveSecretFroMongomeryKey(generalSecretKeyObject, mongomeryPrivateKey, this.template),
            _ => throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID,
                $"Base key handle is invalid, is not EcdsaPrivateKeyObject or MontgomeryPrivateKeyObject.")
        };

        this.logger.LogInformation("Derived secret with length {secretLen}.", secret.Length);

        generalSecretKeyObject.SetSecret(secret);

        if (this.template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            uint requestdValueLen = this.template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN);
            this.TryUpdateValueLen(generalSecretKeyObject, (int)requestdValueLen);
        }

        generalSecretKeyObject.CkaAlwaysSensitive = sBaseKey.CkaAlwaysSensitive;
        generalSecretKeyObject.CkaNewerExtractable = sBaseKey.CkaNewerExtractable;
        generalSecretKeyObject.CkaLocal = false;

        return generalSecretKeyObject;
    }

    public override string ToString()
    {
        return "Ecdh1DeriveKeyGenerator";
    }

    private byte[] DeriveSecret(SecretKeyObject generalSecretKeyObject, EcdsaPrivateKeyObject sBaseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret");

        uint minKeySize = template.GetAttributeUint(CKA.CKA_VALUE_LEN, generalSecretKeyObject.GetMinimalSecretLen());
        IBasicAgreement agreement = this.CreateAgreement(this.ecdh1Params.Kdf,
            (int)minKeySize,
            this.ecdh1Params.SharedData);

        ECPublicKeyParameters publicKey = this.GetPublicKeyFromData(sBaseKey, this.ecdh1Params.PublicKeyData);

        agreement.Init(sBaseKey.GetPrivateKey());
        Org.BouncyCastle.Math.BigInteger agreementIntValue = agreement.CalculateAgreement(publicKey);
        this.logger.LogTrace("Calculated agreement as big integer.");

        return agreementIntValue.ToByteArrayUnsigned();
    }

    private byte[] DeriveSecretFroMongomeryKey(SecretKeyObject generalSecretKeyObject, MontgomeryPrivateKeyObject sBaseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecretFroMongomeryKey");

        uint minKeySize = template.GetAttributeUint(CKA.CKA_VALUE_LEN, generalSecretKeyObject.GetMinimalSecretLen());
        AsymmetricKeyParameter privateKey = sBaseKey.GetPrivateKey();
        if (privateKey is X25519PrivateKeyParameters x25519PrivateKey)
        {
            IRawAgreement basicAgreement = this.CreateAgreement(new X25519Agreement(),
                this.ecdh1Params.Kdf,
                this.ecdh1Params.SharedData);
            basicAgreement.Init(x25519PrivateKey);

            X25519PublicKeyParameters publicKey = new X25519PublicKeyParameters(this.ecdh1Params.PublicKeyData);
            byte[] derivedKey = new byte[minKeySize];
            basicAgreement.CalculateAgreement(publicKey, derivedKey.AsSpan());
            this.logger.LogTrace("Calculated agreement as byte array.");

            return derivedKey;
        }

        if (privateKey is X448PrivateKeyParameters x448PrivateKey)
        {
            IRawAgreement basicAgreement = this.CreateAgreement(new X448Agreement(),
                this.ecdh1Params.Kdf,
                this.ecdh1Params.SharedData);
            basicAgreement.Init(x448PrivateKey);

            X448PublicKeyParameters publicKey = new X448PublicKeyParameters(this.ecdh1Params.PublicKeyData);
            byte[] derivedKey = new byte[minKeySize];
            basicAgreement.CalculateAgreement(publicKey, derivedKey.AsSpan());
            this.logger.LogTrace("Calculated agreement as byte array.");

            return derivedKey;
        }

        this.logger.LogError("Base key handle is invalid is {baseKey}. Excepted X25519PrivateKeyParameters or X448PrivateKeyParameters.", privateKey);
        throw new InvalidDataException("Base key handle is invalid. Excepted X25519PrivateKeyParameters or X448PrivateKeyParameters.");
    }

    private ECPublicKeyParameters GetPublicKeyFromData(EcdsaPrivateKeyObject baseKey, byte[] publicKeyData)
    {
        this.logger.LogTrace("Entering to GetPublicKeyFromData.");
        try
        {
            return EcdsaUtils.ParsePublicKey(baseKey.CkaEcParams, publicKeyData);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during GetPublicKeyFromData.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                "PublicData from CK_ECDH1_DERIVE_PARAMS is not valid public key data.",
                ex);
        }
    }

    protected virtual IBasicAgreement CreateAgreement(CKD kdfFunction, int minKeySize, byte[]? sharedData)
    {
        return this.CreateAgreement(new ECDHBasicAgreement(),
            kdfFunction,
            minKeySize,
            sharedData);
    }

    protected virtual IBasicAgreement CreateAgreement(IBasicAgreement basicAgreement, CKD kdfFunction, int minKeySize, byte[]? sharedData)
    {
        this.logger.LogTrace("Entering to CreateAgreement with KDF {Kdf}, minKeySize {minKeySize}, contains sharedData {containsSharedData}.",
            kdfFunction,
            minKeySize,
            sharedData != null);

        return kdfFunction switch
        {
            CKD.CKD_NULL => basicAgreement,
            CKD.CKD_SHA1_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha1Digest(), sharedData),
            CKD.CKD_SHA224_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha224Digest(), sharedData),
            CKD.CKD_SHA256_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha256Digest(), sharedData),
            CKD.CKD_SHA384_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha384Digest(), sharedData),
            CKD.CKD_SHA512_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha512Digest(), sharedData),
            CKD.CKD_SHA3_224_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(224), sharedData),
            CKD.CKD_SHA3_256_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(256), sharedData),
            CKD.CKD_SHA3_384_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(384), sharedData),
            CKD.CKD_SHA3_512_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Sha3Digest(512), sharedData),
            CKD.CKD_BLAKE2B_160_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(160), sharedData),
            CKD.CKD_BLAKE2B_256_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(256), sharedData),
            CKD.CKD_BLAKE2B_384_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(384), sharedData),
            CKD.CKD_BLAKE2B_512_KDF => new AgreementWithKdf1Agreement(basicAgreement, minKeySize, new Blake2bDigest(512), sharedData),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"kdf {kdfFunction} from CK_ECDH1_DERIVE_PARAMS is not supported or invalid.")
        };
    }

    protected virtual IRawAgreement CreateAgreement(IRawAgreement basicAgreement, CKD kdfFunction, byte[]? sharedData)
    {
        this.logger.LogTrace("Entering to CreateAgreement with KDF {Kdf}, contains sharedData {containsSharedData}.",
            kdfFunction,
            sharedData != null);

        return kdfFunction switch
        {
            CKD.CKD_NULL => new SafeRawAgreement(basicAgreement),
            CKD.CKD_SHA1_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha1Digest(), sharedData),
            CKD.CKD_SHA224_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha224Digest(), sharedData),
            CKD.CKD_SHA256_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha256Digest(), sharedData),
            CKD.CKD_SHA384_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha384Digest(), sharedData),
            CKD.CKD_SHA512_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha512Digest(), sharedData),
            CKD.CKD_SHA3_224_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(224), sharedData),
            CKD.CKD_SHA3_256_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(256), sharedData),
            CKD.CKD_SHA3_384_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(384), sharedData),
            CKD.CKD_SHA3_512_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Sha3Digest(512), sharedData),
            CKD.CKD_BLAKE2B_160_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(160), sharedData),
            CKD.CKD_BLAKE2B_256_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(256), sharedData),
            CKD.CKD_BLAKE2B_384_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(384), sharedData),
            CKD.CKD_BLAKE2B_512_KDF => new RawAgreementWithKdf1Agreement(basicAgreement, new Blake2bDigest(512), sharedData),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"kdf {kdfFunction} from CK_ECDH1_DERIVE_PARAMS is not supported or invalid.")
        };
    }

    protected virtual void CheckTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (template.ContainsKey(CKA.CKA_VALUE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE} can not use in C_GenerateKey.");
        }

        if (template.GetAttributeUint(CKA.CKA_VALUE_LEN, 1) < 1)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE_LEN} is less than 1.");
        }

        if ((CKO)template.GetAttributeUint(CKA.CKA_CLASS, (uint)CKO.CKO_SECRET_KEY) != CKO.CKO_SECRET_KEY)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_CLASS} must by {CKO.CKO_SECRET_KEY}.");
        }

        if (!template.ContainsKey(CKA.CKA_KEY_TYPE) && !template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"For this mechanism must set {CKA.CKA_KEY_TYPE} or {CKA.CKA_VALUE_LEN}.");
        }

        if (!template.ContainsKey(CKA.CKA_VALUE_LEN) && (CKK)template.GetAttributeUint(CKA.CKA_KEY_TYPE, (uint)CKK.CKK_GENERIC_SECRET) != CKK.CKK_GENERIC_SECRET)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Combination {CKA.CKA_KEY_TYPE} {CKK.CKK_GENERIC_SECRET} without defined {CKA.CKA_VALUE_LEN} is not allowed.");
        }
    }

    private void TryUpdateValueLen(SecretKeyObject generalSecretKeyObject, int requestdValueLen)
    {
        this.logger.LogTrace("Entering to TryUpdateValueLen with requestdValueLen {requestdValueLen}.",
            requestdValueLen);

        byte[] secret = generalSecretKeyObject.GetSecret();
        if (secret.Length < requestdValueLen)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
               $"Attribute {nameof(CKA.CKA_VALUE_LEN)} is inconsistent with used mechanism type. {nameof(CKA.CKA_VALUE_LEN)} has value {requestdValueLen}, maximum is {secret.Length}.");
        }

        if (secret.Length > requestdValueLen)
        {
            generalSecretKeyObject.SetSecret(secret.AsSpan(0, requestdValueLen).ToArray());
            this.logger.LogDebug("Secret is concatet to {requestdValueLen}.", requestdValueLen);
        }
    }
}

