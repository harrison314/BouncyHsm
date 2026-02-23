using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class SlhDsaKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private readonly ILogger<SlhDsaKeyPairGenerator> logger;

    public SlhDsaKeyPairGenerator(ILogger<SlhDsaKeyPairGenerator> logger)
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

        CK_SLH_DSA_PARAMETER_SET parameterSet = (CK_SLH_DSA_PARAMETER_SET)this.publicKeyTemplate.GetRequiredAttributeUint(CKA.CKA_PARAMETER_SET);

        (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) = this.GenerateInternal(secureRandom, parameterSet);

        SlhDsaPublicKeyObject pubKeyObject = this.CreatePublicKey(publicKey, this.publicKeyTemplate);
        SlhDsaPrivateKeyObject privKeyObject = this.CreatePrivateKey(privateKey, this.privateKeyTemplate);

        return (pubKeyObject, privKeyObject);
    }

    public override string ToString()
    {
        return "SlhDsaKeyPairGenerator";
    }

    private (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) GenerateInternal(SecureRandom secureRandom, CK_SLH_DSA_PARAMETER_SET parameterSet)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Entering to GenerateInternal with CKA_PARAMETER_SET {parameterSet}.",
                parameterSet);
        }

        SlhDsaParameters parameterSetObject = SlhDsaUtils.GetParametersFromType(parameterSet);
        Org.BouncyCastle.Crypto.Generators.SlhDsaKeyPairGenerator generator = new Org.BouncyCastle.Crypto.Generators.SlhDsaKeyPairGenerator();
        generator.Init(new SlhDsaKeyGenerationParameters(secureRandom, parameterSetObject));

        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        this.logger.LogInformation("Generated SLH-DSA key pair ({slhDsaName})", parameterSet);
        return (keyPair.Public, keyPair.Private);
    }

    private SlhDsaPrivateKeyObject CreatePrivateKey(AsymmetricKeyParameter privateKey, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePrivateKey.");

        SlhDsaPrivateKeyObject privateKeyObject = new SlhDsaPrivateKeyObject();
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

    private SlhDsaPublicKeyObject CreatePublicKey(AsymmetricKeyParameter publicKey, IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePublicKey.");

        SlhDsaPublicKeyObject slhDsaPublicKeyObject = new SlhDsaPublicKeyObject();
        foreach ((CKA attrType, IAttributeValue attrValue) in publicKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            slhDsaPublicKeyObject.SetValue(attrType, attrValue, false);
        }

        slhDsaPublicKeyObject.SetPublicKey(publicKey);

        return slhDsaPublicKeyObject;
    }

    private void CheckTemplates(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CheckTemplates.");

        CK_SLH_DSA_PARAMETER_SET parameterSet = (CK_SLH_DSA_PARAMETER_SET)publicKeyTemplate.GetRequiredAttributeUint(CKA.CKA_PARAMETER_SET);
        if (!Enum.IsDefined<CK_SLH_DSA_PARAMETER_SET>(parameterSet))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
             $"Attribute {CKA.CKA_PARAMETER_SET} is not valid in public key template.");
        }
    }
}