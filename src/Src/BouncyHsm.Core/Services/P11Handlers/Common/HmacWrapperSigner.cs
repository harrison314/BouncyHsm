using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts.Entities;
using Org.BouncyCastle.Security;
using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Crypto.Macs;
using BouncyHsm.Core.Services.Bc;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class HmacWrapperSigner : IWrapperSigner
{
    private readonly HMac hmac;
    private readonly CKM mechanism;
    private readonly CKK? addAllowedKeyType;
    private readonly int? generalParameter;
    private readonly ILogger<HmacWrapperSigner> logger;

    public HmacWrapperSigner(IDigest digest,
        CKM mechanism,
        CKK? addAllowedKeyType,
        int? generalParameter,
        ILogger<HmacWrapperSigner> logger)
    {
        this.hmac = new HMac(digest);
        this.mechanism = mechanism;
        this.addAllowedKeyType = addAllowedKeyType;
        this.generalParameter = generalParameter;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        GenericSecretKeyObject secretKey = this.CheckKey(keyObject);
        if (!secretKey.CkaSign)
        {
            this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", secretKey.Id);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
        }

        ISigner signer = this.CreateSigner();
        signer.Init(true, new KeyParameter(secretKey.GetSecret()));

        return new AuthenticatedSigner(signer, false);
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        GenericSecretKeyObject secretKey = this.CheckKey(keyObject);
        if (!secretKey.CkaVerify)
        {
            this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", secretKey.Id);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
        }

        ISigner signer = this.CreateSigner();
        signer.Init(false, new KeyParameter(secretKey.GetSecret()));

        return signer;
    }

    private ISigner CreateSigner()
    {
        try
        {
            return (this.generalParameter.HasValue)
            ? new HmacGeneralSignerAdapter(this.hmac, this.generalParameter.Value)
            : new HmacSignerAdapter(this.hmac);

        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Mechanism param for {this.mechanism} has invalid length.",
                ex);
        }
    }

    private GenericSecretKeyObject CheckKey(KeyObject keyObject)
    {
        if (keyObject is GenericSecretKeyObject secretKey)
        {
            if (secretKey.CkaKeyType != CKK.CKK_GENERIC_SECRET)
            {
                if (this.addAllowedKeyType.HasValue)
                {
                    if (secretKey.CkaKeyType != this.addAllowedKeyType.Value)
                    {
                        throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} require CKK_GENERIC_SECRET or {this.addAllowedKeyType.Value}.");
                    }
                }
                else
                {
                    throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} require CKK_GENERIC_SECRET.");
                }

            }

            if (!MechanismUtils.TryGetMechanismInfo(this.mechanism, out MechanismInfo mechanismInfo))
            {
                System.Diagnostics.Debug.Fail("Not supported mechanism.");
            }

            if (mechanismInfo.MinKeySize > secretKey.CkaValueLen || mechanismInfo.MaxKeySize < secretKey.CkaValueLen)
            {
                throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
                    $"Mechanism {this.mechanism} require key size between {mechanismInfo.MinKeySize} and {mechanismInfo.MaxKeySize}. Key object {secretKey.Id} has length {secretKey.CkaValueLen}.");
            }

            return secretKey;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required  Generic secret key.");
        }
    }

    public override string ToString()
    {
        return $"HmacWrapperSigner (mechanism {this.mechanism}, algorithm {this.hmac.AlgorithmName})";
    }
}
