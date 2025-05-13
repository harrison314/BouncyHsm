using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

/// <summary>
/// ED key pair generator for <seealso cref="CKM.CKM_EC_MONTGOMERY_KEY_PAIR_GEN"/>.
/// </summary>
internal class MontgomeryKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private readonly ILogger<MontgomeryKeyPairGenerator> logger;

    public MontgomeryKeyPairGenerator(ILogger<MontgomeryKeyPairGenerator> logger)
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

        MontgomeryPublicKeyObject pubKeyObject = this.CreatePublicKey(publicKey, this.publicKeyTemplate);
        MontgomeryPrivateKeyObject privKeyObject = this.CreatePrivateKey(privateKey, this.privateKeyTemplate);

        return (pubKeyObject, privKeyObject);
    }

    public override string ToString()
    {
        return "MontgomeryKeyPairGenerator";
    }

    private (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) GenerateInternal(SecureRandom secureRandom, byte[] ecParams)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Entering to GenerateInternal with CKA_EC_PARAMS {ecParamsBin}.",
                Convert.ToHexString(ecParams));
        }

        DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(ecParams);

        IAsymmetricCipherKeyPairGenerator generator;
        if (oid.Equals(EdECObjectIdentifiers.id_X25519))
        {
            generator = new Org.BouncyCastle.Crypto.Generators.X25519KeyPairGenerator();
        }
        else if (oid.Equals(EdECObjectIdentifiers.id_X448))
        {
            generator = new Org.BouncyCastle.Crypto.Generators.X448KeyPairGenerator();
        }
        else
        {
            throw new InvalidProgramException($"OID {oid} is not supported for public montgomery key.");
        }

        generator.Init(new KeyGenerationParameters(secureRandom, 0));
        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        this.logger.LogInformation("Generated montgomery key pair for named curve {NamedCurveOid}", MontgomeryEcUtils.ParseEcParamsAsName(ecParams));
        return (keyPair.Public, keyPair.Private);
    }

    private MontgomeryPrivateKeyObject CreatePrivateKey(AsymmetricKeyParameter privateKey, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePrivateKey.");

        MontgomeryPrivateKeyObject privateKeyObject = new MontgomeryPrivateKeyObject(CKM.CKM_EC_MONTGOMERY_KEY_PAIR_GEN);

        foreach ((CKA attrType, IAttributeValue attrValue) in privateKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            privateKeyObject.SetValue(attrType, attrValue, false);
        }

        privateKeyObject.SetPrivateKey(privateKey);

        return privateKeyObject;
    }

    private MontgomeryPublicKeyObject CreatePublicKey(AsymmetricKeyParameter publicKey, IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePublicKey.");

        MontgomeryPublicKeyObject edPublicKeyObject = new MontgomeryPublicKeyObject(CKM.CKM_EC_MONTGOMERY_KEY_PAIR_GEN);

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
        edPublicKeyObject.CkaEcParams = publicKeyTemplate.GetRequiredAttributeBytes(CKA.CKA_EC_PARAMS);

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