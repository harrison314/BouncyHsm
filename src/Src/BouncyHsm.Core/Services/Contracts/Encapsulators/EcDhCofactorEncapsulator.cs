using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal class EcDhCofactorEncapsulator : P11EncapsulatorBase<EcdsaPublicKeyObject, EcdsaPrivateKeyObject>
{
    private readonly Ecdh1DeriveParamsWithoutPublicKey ecDeriveParams;

    public EcDhCofactorEncapsulator(Ecdh1DeriveParamsWithoutPublicKey ecDeriveParams, ILogger<EcDhCofactorEncapsulator> logger)
        : base(logger, CKM.CKM_ECDH1_COFACTOR_DERIVE)
    {
        this.ecDeriveParams = ecDeriveParams;
    }

    protected override void EncapsulateInternal(EcdsaPublicKeyObject publicKey, SecretKeyObject secretKeyObject, SecureRandom secureRandom, out byte[] encapsulatedData)
    {
        this.CheckEcdh1DeriveParams();

        (ECPublicKeyParameters epheralPublicKey, ECPrivateKeyParameters epheralPrivateKey) = this.GenerateEpheral(secureRandom, publicKey);

        IBasicAgreement agreement = BouncyHsm.Core.Services.Contracts.Generators.AgreementUtils.CreateAgreement(new ECDHCBasicAgreement(),
            (CKD)this.ecDeriveParams.Kdf,
            this.GetMinimalSecretLength(),
            this.ecDeriveParams.SharedData);

        agreement.Init(epheralPrivateKey);
        BigInteger sharedSecret = agreement.CalculateAgreement((ECPublicKeyParameters)publicKey.GetPublicKey());
        this.SetSecretKeyPadded(secretKeyObject, sharedSecret.ToByteArrayUnsigned());
        encapsulatedData = EcdsaUtils.EncodeP11EcPoint(epheralPublicKey.Q);
    }

    protected override void DecapsulateInternal(EcdsaPrivateKeyObject privateKey, byte[] encapsulatedData, SecretKeyObject secretKeyObject)
    {
        this.CheckEcdh1DeriveParams();

        ECPublicKeyParameters publicKeyParams = this.GetPublicKeyFromData(privateKey, encapsulatedData);

        IBasicAgreement agreement = BouncyHsm.Core.Services.Contracts.Generators.AgreementUtils.CreateAgreement(new ECDHCBasicAgreement(),
            (CKD)this.ecDeriveParams.Kdf,
            this.GetMinimalSecretLength(),
            this.ecDeriveParams.SharedData);

        agreement.Init(privateKey.GetPrivateKey());

        BigInteger sharedSecret = agreement.CalculateAgreement(publicKeyParams);
        this.SetSecretKeyPadded(secretKeyObject, sharedSecret.ToByteArrayUnsigned());
    }

    protected override int GetEncapsulatedDataLengthInternal(EcdsaPublicKeyObject publicKey)
    {
        return publicKey.CkaEcPoint.Length;
    }

    private (ECPublicKeyParameters publicKey, ECPrivateKeyParameters privateKey) GenerateEpheral(SecureRandom secureRandom, EcdsaPublicKeyObject publicKey)
    {
        if (this.logger.IsEnabled(LogLevel.Trace))
        {
            this.logger.LogTrace("Entering to GenerateEpheral with CKA_EC_PARAMS {ecParamsBin}.",
                Convert.ToHexString(publicKey.CkaEcParams));
        }

        ECKeyGenerationParameters ecKeyGenParams = EcdsaUtils.ParseEcParamsToECKeyGenerationParameters(publicKey.CkaEcParams, secureRandom);

        Org.BouncyCastle.Crypto.Generators.ECKeyPairGenerator ecKeyPairGenerator = new Org.BouncyCastle.Crypto.Generators.ECKeyPairGenerator();
        ecKeyPairGenerator.Init(ecKeyGenParams);
        Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = ecKeyPairGenerator.GenerateKeyPair();

        this.logger.LogInformation("Generated epheral EC key pair for named curve {NamedCurveOid}", EcdsaUtils.ParseEcParamsAsName(publicKey.CkaEcParams));

        return ((ECPublicKeyParameters)keyPair.Public, (ECPrivateKeyParameters)keyPair.Private);
    }

    private void CheckEcdh1DeriveParams()
    {
        if (this.ecDeriveParams.PublicKeyData != null && this.ecDeriveParams.PublicKeyData.Length > 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                "When mechanism CKM_ECDH1_COFACTOR_DERIVE is used in C_EncapsulateKey and C_DecapsulateKey, the mechanism parameters pPublicData and ulPublicDataLen must be set to NULL and 0 respectively.");
        }
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
            throw new RpcPkcs11Exception(CKR.CKR_ARGUMENTS_BAD,
                "PublicData from chiphertext is not valid.",
                ex);
        }
    }
}
