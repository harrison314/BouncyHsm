using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Generators;

/// <summary>
/// RSA key pair generator for <seealso cref="CKM.CKM_RSA_PKCS_KEY_PAIR_GEN"/>.
/// </summary>
internal class RsaKeyPairGenerator : IKeyPairGenerator
{
    private IReadOnlyDictionary<CKA, IAttributeValue>? publicKeyTemplate;
    private IReadOnlyDictionary<CKA, IAttributeValue>? privateKeyTemplate;
    private readonly ILogger<RsaKeyPairGenerator> logger;

    public RsaKeyPairGenerator(ILogger<RsaKeyPairGenerator> logger)
    {
        this.logger = logger;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to init");

        this.CheckTemplates(publicKeyTemplate, privateKeyTemplate);

        this.publicKeyTemplate = publicKeyTemplate;
        this.privateKeyTemplate = privateKeyTemplate;
    }

    public (PublicKeyObject publicKey, PrivateKeyObject privateKey) Generate(SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to Generate");

        System.Diagnostics.Debug.Assert(this.publicKeyTemplate != null);
        System.Diagnostics.Debug.Assert(this.privateKeyTemplate != null);

        byte[] publicExponent = this.publicKeyTemplate.GetAttributeBytes(CKA.CKA_PUBLIC_EXPONENT, new byte[] { 0x01, 0x00, 0x01 });
        uint modulusBits = this.publicKeyTemplate.GetRequiredAttributeUint(CKA.CKA_MODULUS_BITS);

        (RsaKeyParameters publicKey, RsaPrivateCrtKeyParameters privateKey) = this.GenerateInternal(secureRandom, publicExponent, modulusBits);

        RsaPublicKeyObject publicKeyObject = this.CreatePublicKeyObject(this.publicKeyTemplate, publicKey);
        RsaPrivateKeyObject privateKeyObject = this.CreatePrivateKeyObject(this.privateKeyTemplate, privateKey);

        return (publicKeyObject, privateKeyObject);
    }

    public override string ToString()
    {
        return "RsaKeyPairGenerator";
    }

    private (RsaKeyParameters publicKey, RsaPrivateCrtKeyParameters privateKey) GenerateInternal(SecureRandom secureRandom, byte[] publicExponent, uint modulusBits)
    {
        this.logger.LogTrace("Entering to GenerateInternal with modulusBits {modulusBits}.", modulusBits);

        bool foundMechanism = MechanismUtils.TryGetMechanismInfo(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN, out MechanismInfo mechanismInfo);
        System.Diagnostics.Debug.Assert(foundMechanism);

        if (modulusBits < mechanismInfo.MinKeySize || modulusBits > mechanismInfo.MaxKeySize || modulusBits % 1024 != 0)
        {
            this.logger.LogError("Modulus bit is invalid value modulusBits {modulusBits}.", modulusBits);
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_VALUE_INVALID, $"Modulus bit is invalid value modulusBits {modulusBits}.");
        }

        Org.BouncyCastle.Crypto.Generators.RsaKeyPairGenerator generator = new Org.BouncyCastle.Crypto.Generators.RsaKeyPairGenerator();
        generator.Init(new RsaKeyGenerationParameters(new Org.BouncyCastle.Math.BigInteger(1, publicExponent),
            secureRandom,
            (int)modulusBits,
            100));

        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = generator.GenerateKeyPair();

        this.logger.LogDebug("RSA key pair with modulusBits {modulusBits} generated.", modulusBits);

        return ((RsaKeyParameters)keyPair.Public, (RsaPrivateCrtKeyParameters)keyPair.Private);
    }

    private RsaPublicKeyObject CreatePublicKeyObject(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, RsaKeyParameters publicKey)
    {
        this.logger.LogTrace("Entering to CreatePublicKeyObject");

        RsaPublicKeyObject publicKeyObject = new RsaPublicKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);

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

    private RsaPrivateKeyObject CreatePrivateKeyObject(IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate, RsaPrivateCrtKeyParameters privateKey)
    {
        this.logger.LogTrace("Entering to CreatePrivateKeyObject");

        RsaPrivateKeyObject privateKeyObject = new RsaPrivateKeyObject(CKM.CKM_RSA_PKCS_KEY_PAIR_GEN);

        foreach ((CKA attrType, IAttributeValue attrValue) in privateKeyTemplate)
        {
            if (attrType is CKA.CKA_CLASS or CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            privateKeyObject.SetValue(attrType, attrValue, false);
        }

        privateKeyObject.SetPrivateKey(privateKey);

        privateKeyObject.CkaAlwaysSensitive = privateKeyObject.CkaSensitive;
        privateKeyObject.CkaAlwaysAuthenticate = false;

        return privateKeyObject;
    }

    private void CheckTemplates(IReadOnlyDictionary<CKA, IAttributeValue> publicKeyTemplate, IReadOnlyDictionary<CKA, IAttributeValue> privateKeyTemplate)
    {
        this.logger.LogTrace("Entering to CheckTemplates");

        IAttributeValue? keyTypeAttribute;
        if (publicKeyTemplate.TryGetValue(CKA.CKA_KEY_TYPE, out keyTypeAttribute) && !keyTypeAttribute.Equals((uint)CKK.CKK_RSA))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"In public key template CKA_KEY_TYPE is not CKK_RSA.");
        }

        if (privateKeyTemplate.TryGetValue(CKA.CKA_KEY_TYPE, out keyTypeAttribute) && !keyTypeAttribute.Equals((uint)CKK.CKK_RSA))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"In private key template CKA_KEY_TYPE is not CKK_RSA.");
        }

        //TODO: Implement other checks
    }
}
