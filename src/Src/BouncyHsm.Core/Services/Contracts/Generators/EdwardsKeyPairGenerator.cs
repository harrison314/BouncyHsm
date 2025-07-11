using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

/// <summary>
/// ED key pair generator for <seealso cref="CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN"/>.
/// </summary>
internal class EdwardsKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private readonly ILogger<EdwardsKeyPairGenerator> logger;

    public EdwardsKeyPairGenerator(ILogger<EdwardsKeyPairGenerator> logger)
    {
        this.logger = logger;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to Init");

        this.CheckTemplates(publicKeyTemplate, privateKeyTemplate);

        this.publicKeyTemplate = publicKeyTemplate;
        this.privateKeyTemplate = privateKeyTemplate;
    }

    public (PublicKeyObject publicKey, PrivateKeyObject privateKey) Generate(SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to Generate");

        System.Diagnostics.Debug.Assert(this.publicKeyTemplate != null);
        System.Diagnostics.Debug.Assert(this.privateKeyTemplate != null);

        byte[] ecParams = this.publicKeyTemplate.GetRequiredAttributeBytes(CKA.CKA_EC_PARAMS);

        (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) = this.GenerateInternal(secureRandom, ecParams);

        EdwardsPublicKeyObject pubKeyObject = this.CreatePublicKey(publicKey, this.publicKeyTemplate, ecParams);
        EdwardsPrivateKeyObject privKeyObject = this.CreatePrivateKey(privateKey, this.privateKeyTemplate, ecParams);

        return (pubKeyObject, privKeyObject);
    }

    public override string ToString()
    {
        return "EcdsaKeyPairGenerator";
    }

    private (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) GenerateInternal(SecureRandom secureRandom, byte[] ecParams)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Entering to GenerateInternal with CKA_EC_PARAMS {ecParamsBin}.",
                Convert.ToHexString(ecParams));
        }

        DerObjectIdentifier oid = EdEcUtils.GetOidFromParams(ecParams);

        IAsymmetricCipherKeyPairGenerator generator;
        if (oid.Equals(EdECObjectIdentifiers.id_Ed25519))
        {
            generator = new Org.BouncyCastle.Crypto.Generators.Ed25519KeyPairGenerator();
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_Ed448))
        {
            generator = new Org.BouncyCastle.Crypto.Generators.Ed448KeyPairGenerator();
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public ED key.");
        }

        generator.Init(new KeyGenerationParameters(secureRandom, 0));
        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        this.logger.LogInformation("Generated ED key pair for named curve {NamedCurveOid}", EdEcUtils.ParseEcParamsAsName(ecParams));
        return (keyPair.Public, keyPair.Private);
    }

    private EdwardsPrivateKeyObject CreatePrivateKey(AsymmetricKeyParameter privateKey, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate, byte[] ecParams)
    {
        this.logger.LogTrace("Entering to CreatePrivateKey.");

        EdwardsPrivateKeyObject privateKeyObject = new EdwardsPrivateKeyObject(CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN);

        foreach ((CKA attrType, IAttributeValue attrValue) in privateKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            privateKeyObject.SetValue(attrType, attrValue, false);
        }

        privateKeyObject.SetPrivateKey(privateKey);
        // Overide the CKA_EC_PARAMS with the one from the template,
        // SetPrivateKey() set CKA_EC_PARAMS with compiuted value
        privateKeyObject.CkaEcParams = ecParams;

        return privateKeyObject;
    }

    private EdwardsPublicKeyObject CreatePublicKey(AsymmetricKeyParameter publicKey, IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, byte[] ecParams)
    {
        this.logger.LogTrace("Entering to CreatePublicKey.");

        EdwardsPublicKeyObject edPublicKeyObject = new EdwardsPublicKeyObject(CKM.CKM_EC_EDWARDS_KEY_PAIR_GEN);

        foreach ((CKA attrType, IAttributeValue attrValue) in publicKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            edPublicKeyObject.SetValue(attrType, attrValue, false);
        }

        edPublicKeyObject.SetPublicKey(publicKey);
        // Overide the CKA_EC_PARAMS with the one from the template,
        // SetPublicKey() set CKA_EC_PARAMS with compiuted value
        edPublicKeyObject.CkaEcParams = ecParams;

        return edPublicKeyObject;
    }

    private void CheckTemplates(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CheckTemplates.");

        byte[] ecParams = publicKeyTemplate.GetRequiredAttributeBytes(CKA.CKA_EC_PARAMS);
        if (privateKeyTemplate.ContainsKey(CKA.CKA_EC_PARAMS))
        {
            byte[] privateEcParams = privateKeyTemplate.GetRequiredAttributeBytes(CKA.CKA_EC_PARAMS);

            if (!ecParams.SequenceEqual(privateEcParams))
            {
                throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"CKA_EC_PARAMS is different in public key and private key template.");
            }
        }
    }
}
