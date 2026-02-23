using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class MlKemKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private readonly ILogger<MlKemKeyPairGenerator> logger;

    public MlKemKeyPairGenerator(ILogger<MlKemKeyPairGenerator> logger)
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

        CK_ML_KEM_PARAMETER_SET parameterSet = (CK_ML_KEM_PARAMETER_SET)this.publicKeyTemplate.GetRequiredAttributeUint(CKA.CKA_PARAMETER_SET);

        (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) = this.GenerateInternal(secureRandom, parameterSet);

        MlKemPublicKeyObject pubKeyObject = this.CreatePublicKey(publicKey, this.publicKeyTemplate);
        MlKemPrivateKeyObject privKeyObject = this.CreatePrivateKey(privateKey, this.privateKeyTemplate);

        return (pubKeyObject, privKeyObject);
    }

    public override string ToString()
    {
        return "MlKemKeyPairGenerator";
    }

    private (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) GenerateInternal(SecureRandom secureRandom, CK_ML_KEM_PARAMETER_SET parameterSet)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Entering to GenerateInternal with CK_ML_KEM_PARAMETER_SET {parameterSet}.",
                parameterSet);
        }

        MLKemParameters parameterSetObject = MlKemUtils.GetParametersFromType(parameterSet);
        MLKemKeyPairGenerator generator = new MLKemKeyPairGenerator();
        generator.Init(new MLKemKeyGenerationParameters(secureRandom, parameterSetObject));

        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        this.logger.LogInformation("Generated ML-KEM key pair ({mlKemName})", parameterSet);
        return (keyPair.Public, keyPair.Private);
    }

    private MlKemPrivateKeyObject CreatePrivateKey(AsymmetricKeyParameter privateKey, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePrivateKey.");

        MlKemPrivateKeyObject privateKeyObject = new MlKemPrivateKeyObject();
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

    private MlKemPublicKeyObject CreatePublicKey(AsymmetricKeyParameter publicKey, IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePublicKey.");

        MlKemPublicKeyObject publicKeyObject = new MlKemPublicKeyObject();
        foreach ((CKA attrType, IAttributeValue attrValue) in publicKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            publicKeyObject.SetValue(attrType, attrValue, false);
        }

        publicKeyObject.SetPublicKey(publicKey);

        return publicKeyObject;
    }

    private void CheckTemplates(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CheckTemplates.");

        CK_ML_KEM_PARAMETER_SET parameterSet = (CK_ML_KEM_PARAMETER_SET)publicKeyTemplate.GetRequiredAttributeUint(CKA.CKA_PARAMETER_SET);
        if (!Enum.IsDefined<CK_ML_KEM_PARAMETER_SET>(parameterSet))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
             $"Attribute {CKA.CKA_PARAMETER_SET} is not valid in public key template.");
        }
    }
}