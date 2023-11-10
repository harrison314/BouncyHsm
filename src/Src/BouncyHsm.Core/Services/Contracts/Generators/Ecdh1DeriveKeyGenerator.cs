using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
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

        if (baseKey is not EcdsaPrivateKeyObject)
        {
            this.logger.LogError("Base key handle is invalid  is {baseKey}. Excepted EcdsaPrivateKeyObject.", baseKey);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID,
              $"Base key handle is invalid, is not EcdsaPrivateKeyObject.");
        }

        EcdsaPrivateKeyObject sBaseKey = (EcdsaPrivateKeyObject)baseKey;

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

        byte[] secret = this.DeriveSecret(generalSecretKeyObject, sBaseKey, this.template);
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
        ECDHBasicAgreement agreement = this.CreateAgreement(this.ecdh1Params.Kdf,
            (int)minKeySize,
            this.ecdh1Params.SharedData);

        ECPublicKeyParameters publicKey = this.GetPublicKeyFromData(sBaseKey, this.ecdh1Params.PublicKeyData);

        agreement.Init(sBaseKey.GetPrivateKey());
        Org.BouncyCastle.Math.BigInteger agreementIntValue = agreement.CalculateAgreement(publicKey);
        this.logger.LogTrace("Calculated agreement as big integer.");

        return agreementIntValue.ToByteArrayUnsigned();
    }

    private ECPublicKeyParameters GetPublicKeyFromData(EcdsaPrivateKeyObject baseKey, byte[] publicKeyData)
    {
        this.logger.LogTrace("Entering to GetPublicKeyFromData.");
        try
        {
            Org.BouncyCastle.Asn1.X9.X9ECParameters ecParams = EcdsaUtils.ParseEcParams(baseKey.CkaEcParams);

            return new ECPublicKeyParameters(EcdsaUtils.DecodeP11EcPoint(ecParams, publicKeyData),
                new ECDomainParameters(ecParams));
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during GetPublicKeyFromData.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                "PublicData from CK_ECDH1_DERIVE_PARAMS is not valid public key data.",
                ex);
        }
    }

    private ECDHBasicAgreement CreateAgreement(CKD kdfFunction, int minKeySize, byte[]? sharedData)
    {
        this.logger.LogTrace("Entering to CreateAgreement with KDF {Kdf}, minKeySize {minKeySize}, contains sharedData {containsSharedData}.",
            kdfFunction,
            minKeySize,
            sharedData != null);

        return kdfFunction switch
        {
            CKD.CKD_NULL => new ECDHBasicAgreement(),
            CKD.CKD_SHA1_KDF => new ECDH1WithKdf1Agreement(minKeySize, new Sha1Digest(), sharedData),
            CKD.CKD_SHA224_KDF => new ECDH1WithKdf1Agreement(minKeySize, new Sha224Digest(), sharedData),
            CKD.CKD_SHA256_KDF => new ECDH1WithKdf1Agreement(minKeySize, new Sha256Digest(), sharedData),
            CKD.CKD_SHA384_KDF => new ECDH1WithKdf1Agreement(minKeySize, new Sha384Digest(), sharedData),
            CKD.CKD_SHA512_KDF => new ECDH1WithKdf1Agreement(minKeySize, new Sha512Digest(), sharedData),
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