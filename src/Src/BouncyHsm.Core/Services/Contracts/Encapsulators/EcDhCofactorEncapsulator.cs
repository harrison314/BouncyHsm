using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

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
        this.SetSecretKeyPadded(secretKeyObject, sharedSecret);
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
        this.SetSecretKeyPadded(secretKeyObject, sharedSecret);
    }

    protected override int GetEncapsulatedDataLengthInternal(EcdsaPublicKeyObject publicKey)
    {
        return publicKey.CkaEcPoint.Length;
    }

    protected void SetSecretKeyPadded(SecretKeyObject secretKeyObject, BigInteger sharedSecret)
    {
        this.logger.LogTrace("Entering to SetSecretKeyPadded.");

        uint? requiredSecretkeyLen = secretKeyObject.GetRequiredSecretLen();
        this.logger.LogTrace("Key type {keyType} required secret length {requiredSecretkeyLen}, template define secret length {templateSecretLength}.",
            secretKeyObject.CkaKeyType,
            requiredSecretkeyLen,
            this.requestedKeyLenusingAttribute);

        if (!requiredSecretkeyLen.HasValue)
        {
            requiredSecretkeyLen = this.requestedKeyLenusingAttribute;
        }

        byte[] secret = sharedSecret.ToByteArrayUnsigned();
        if (requiredSecretkeyLen.HasValue)
        {
            if (secret.Length == requiredSecretkeyLen.Value)
            {
                secretKeyObject.SetSecret(secret);
                return;
            }

            if (secret.Length > requiredSecretkeyLen.Value)
            {
                byte[] truncatedSecret = new byte[requiredSecretkeyLen.Value];
                Array.Copy(secret, 0, truncatedSecret, 0, truncatedSecret.Length);
                secretKeyObject.SetSecret(truncatedSecret);

                this.logger.LogInformation("Derived secret key was truncated from {OriginalLen} to {TruncatedLen} for {KeyObject}.",
                    secret.Length,
                    truncatedSecret.Length,
                    secretKeyObject.GetType().Name);
                return;
            }

            secretKeyObject.SetSecret(BigIntegers.AsUnsignedByteArray((int)requiredSecretkeyLen.Value, sharedSecret));
        }

        uint minimalSecretLen = secretKeyObject.GetMinimalSecretLen();
        if (secret.Length < minimalSecretLen)
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
               $"The generated secret key length {secret.Length} is less than minmal length {minimalSecretLen} for key {secretKeyObject.GetType().Name} ({secretKeyObject.CkaKeyType}).");
        }

        secretKeyObject.SetSecret(secret);
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
