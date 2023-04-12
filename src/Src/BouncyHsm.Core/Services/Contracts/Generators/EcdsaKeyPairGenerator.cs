using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Generators;

/// <summary>
/// EC DSA key pair generator for <seealso cref="CKM.CKM_ECDSA_KEY_PAIR_GEN"/>.
/// </summary>
internal class EcdsaKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private readonly ILogger<EcdsaKeyPairGenerator> logger;

    public EcdsaKeyPairGenerator(ILogger<EcdsaKeyPairGenerator> logger)
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

        (ECPublicKeyParameters publicKey, ECPrivateKeyParameters privateKey) = this.GenerateInternal(secureRandom, ecParams);

        EcdsaPublicKeyObject pubKeyObject = this.CreatePublicKey(publicKey, this.publicKeyTemplate);
        EcdsaPrivateKeyObject privKeyObject = this.CreatePrivateKey(privateKey, this.privateKeyTemplate);


        return (pubKeyObject, privKeyObject);
    }

    public override string ToString()
    {
        return "EcdsaKeyPairGenerator";
    }

    private (ECPublicKeyParameters publicKey, ECPrivateKeyParameters privateKey) GenerateInternal(SecureRandom secureRandom, byte[] ecParams)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Entering to GenerateInternal with CKA_EC_PARAMS {ecParamsBin}.",
                BitConverter.ToString(ecParams));
        }

        DerObjectIdentifier namedCurve = EcdsaUtils.ParseEcParamsOid(ecParams);

        Org.BouncyCastle.Crypto.Generators.ECKeyPairGenerator ecKeyPairGenerator = new Org.BouncyCastle.Crypto.Generators.ECKeyPairGenerator();
        ecKeyPairGenerator.Init(new ECKeyGenerationParameters(namedCurve, secureRandom));
        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = ecKeyPairGenerator.GenerateKeyPair();

        this.logger.LogInformation("Generated EC key pair for named curve {NamedCurveOid}", namedCurve);

        return ((ECPublicKeyParameters)keyPair.Public, (ECPrivateKeyParameters)keyPair.Private);
    }

    private EcdsaPrivateKeyObject CreatePrivateKey(ECPrivateKeyParameters privateKey, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePrivateKey.");

        EcdsaPrivateKeyObject privateKeyObject = new EcdsaPrivateKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN);

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

    private EcdsaPublicKeyObject CreatePublicKey(ECPublicKeyParameters publicKey, IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate)
    {
        this.logger.LogTrace("Entering to CreatePublicKey.");

        EcdsaPublicKeyObject ecdsaPublicKeyObject = new EcdsaPublicKeyObject(CKM.CKM_ECDSA_KEY_PAIR_GEN);

        foreach ((CKA attrType, IAttributeValue attrValue) in publicKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            ecdsaPublicKeyObject.SetValue(attrType, attrValue, false);
        }

        ecdsaPublicKeyObject.SetPublicKey(publicKey);

        return ecdsaPublicKeyObject;
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
