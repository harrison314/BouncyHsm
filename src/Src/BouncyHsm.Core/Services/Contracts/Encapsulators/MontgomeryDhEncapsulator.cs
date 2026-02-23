using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.EdEC;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal class MontgomeryDhEncapsulator : P11EncapsulatorBase<MontgomeryPublicKeyObject, MontgomeryPrivateKeyObject>
{
    private readonly Ecdh1DeriveParamsWithoutPublicKey ecDeriveParams;

    public MontgomeryDhEncapsulator(Ecdh1DeriveParamsWithoutPublicKey ecDeriveParams, ILogger<MontgomeryDhEncapsulator> logger)
        : base(logger, CKM.CKM_ECDH1_DERIVE)
    {
        this.ecDeriveParams = ecDeriveParams;
    }

    protected override void EncapsulateInternal(MontgomeryPublicKeyObject publicKey, SecretKeyObject secretKeyObject, SecureRandom secureRandom, out byte[] encapsulatedData)
    {
        this.CheckEcdh1DeriveParams();

        (AsymmetricKeyParameter epheralPublicKey, AsymmetricKeyParameter epheralPrivateKey) = this.GenerateEpheral(secureRandom, publicKey);

        byte[] derivedKey = new byte[this.GetMinimalSecretLength()];
        if (epheralPrivateKey is X25519PrivateKeyParameters x25519PrivateKey)
        {
            IRawAgreement basicAgreement = AgreementUtils.CreateAgreement(new X25519Agreement(),
                this.ecDeriveParams.Kdf,
                this.ecDeriveParams.SharedData);
            basicAgreement.Init(x25519PrivateKey);

            X25519PublicKeyParameters publicKeyParams = new X25519PublicKeyParameters(publicKey.CkaEcPoint);

            basicAgreement.CalculateAgreement(publicKeyParams, derivedKey.AsSpan());
        }

        if (epheralPrivateKey is X448PrivateKeyParameters x448PrivateKey)
        {
            IRawAgreement basicAgreement = AgreementUtils.CreateAgreement(new X448Agreement(),
                this.ecDeriveParams.Kdf,
                this.ecDeriveParams.SharedData);
            basicAgreement.Init(x448PrivateKey);

            X448PublicKeyParameters publicKeyParams = new X448PublicKeyParameters(publicKey.CkaEcPoint);
            basicAgreement.CalculateAgreement(publicKeyParams, derivedKey.AsSpan());
            this.logger.LogTrace("Calculated agreement as byte array.");
        }

        this.SetSecretKeyPadded(secretKeyObject, derivedKey);

        if (epheralPublicKey is X25519PublicKeyParameters x25519PublicKey)
        {
            encapsulatedData = x25519PublicKey.GetEncoded();
        }
        else if (epheralPublicKey is X448PublicKeyParameters x448PublicKey)
        {
            encapsulatedData = x448PublicKey.GetEncoded();
        }
        else
        {
            throw new InvalidProgramException("Epheral public key is of unknown type.");
        }
    }

    protected override void DecapsulateInternal(MontgomeryPrivateKeyObject privateKey, byte[] encapsulatedData, SecretKeyObject secretKeyObject)
    {
        this.CheckEcdh1DeriveParams();

        AsymmetricKeyParameter publicKeyParams = this.GetPublicKeyFromData(privateKey, encapsulatedData);
        AsymmetricKeyParameter privateKeyParamaters = privateKey.GetPrivateKey();

        byte[] derivedKey = new byte[this.GetMinimalSecretLength()];
        if (privateKeyParamaters is X25519PrivateKeyParameters x25519PrivateKey)
        {
            IRawAgreement basicAgreement = AgreementUtils.CreateAgreement(new X25519Agreement(),
                this.ecDeriveParams.Kdf,
                this.ecDeriveParams.SharedData);
            basicAgreement.Init(x25519PrivateKey);

            basicAgreement.CalculateAgreement(publicKeyParams, derivedKey.AsSpan());
        }

        if (privateKeyParamaters is X448PrivateKeyParameters x448PrivateKey)
        {
            IRawAgreement basicAgreement = AgreementUtils.CreateAgreement(new X448Agreement(),
                this.ecDeriveParams.Kdf,
                this.ecDeriveParams.SharedData);
            basicAgreement.Init(x448PrivateKey);

            basicAgreement.CalculateAgreement(publicKeyParams, derivedKey.AsSpan());
        }

        this.SetSecretKeyPadded(secretKeyObject, derivedKey);
    }

    protected override int GetEncapsulatedDataLengthInternal(MontgomeryPublicKeyObject publicKey)
    {
        return publicKey.CkaEcPoint.Length;
    }

    private (AsymmetricKeyParameter publicKey, AsymmetricKeyParameter privateKey) GenerateEpheral(SecureRandom secureRandom, MontgomeryPublicKeyObject publicKey)
    {
        DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(publicKey.CkaEcParams);

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

        return (keyPair.Public, keyPair.Private);
    }

    private void CheckEcdh1DeriveParams()
    {
        if (this.ecDeriveParams.PublicKeyData != null && this.ecDeriveParams.PublicKeyData.Length > 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                "When mechanism CKM_ECDH1_DERIVE is used in C_EncapsulateKey and C_DecapsulateKey, the mechanism parameters pPublicData and ulPublicDataLen must be set to NULL and 0 respectively.");
        }
    }

    private AsymmetricKeyParameter GetPublicKeyFromData(MontgomeryPrivateKeyObject baseKey, byte[] publicKeyData)
    {
        this.logger.LogTrace("Entering to GetPublicKeyFromData.");
        try
        {
            DerObjectIdentifier oid = MontgomeryEcUtils.GetOidFromParams(baseKey.CkaEcParams);

            if (oid.Equals(EdECObjectIdentifiers.id_X25519))
            {
                return new X25519PublicKeyParameters(publicKeyData);
            }
            else if (oid.Equals(EdECObjectIdentifiers.id_X448))
            {
                return new X448PublicKeyParameters(publicKeyData);
            }
            else
            {
                throw new InvalidProgramException($"OID {oid} is not supported for public montgomery key.");
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during GetPublicKeyFromData.");
            throw new RpcPkcs11Exception(CKR.CKR_ARGUMENTS_BAD,
                "PublicData from chiphertext is not valid.",
                ex);
        }
    }
}