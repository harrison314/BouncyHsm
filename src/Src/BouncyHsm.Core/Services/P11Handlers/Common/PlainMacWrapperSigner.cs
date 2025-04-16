using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts.Entities;
using Org.BouncyCastle.Security;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Bc;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class PlainMacWrapperSigner : IWrapperSigner
{
    private readonly CKM mechanism;
    private readonly IMac macAlgorithm;
    private readonly ILogger<PlainMacWrapperSigner> logger;
    private readonly CKK? addAllowedKeyType;

    public PlainMacWrapperSigner(CKM mechanism,
        IMac macAlgorithm,
        ILogger<PlainMacWrapperSigner> logger,
        CKK? addAllowedKeyType)
    {
        this.mechanism = mechanism;
        this.macAlgorithm = macAlgorithm;
        this.logger = logger;
        this.addAllowedKeyType = addAllowedKeyType;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        SecretKeyObject secretKey = this.CheckKey(keyObject);
        if (!secretKey.CkaSign)
        {
            this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", secretKey.Id);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
        }

        try
        {
            HmacSignerAdapter signer = new HmacSignerAdapter(this.macAlgorithm);
            signer.Init(true, new KeyParameter(secretKey.GetSecret()));

            return new AuthenticatedSigner(signer, false);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Mechanism param for {this.mechanism} has invalid length.",
                ex);
        }
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        SecretKeyObject secretKey = this.CheckKey(keyObject);

        if (!secretKey.CkaVerify)
        {
            this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", secretKey.Id);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
        }
        try
        {
            HmacSignerAdapter signer = new HmacSignerAdapter(this.macAlgorithm);
            signer.Init(false, new KeyParameter(secretKey.GetSecret()));

            return signer;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Mechanism param for {this.mechanism} has invalid length.",
                ex);
        }
    }

    public override string ToString()
    {
        return $"PlainMacWrapperSigner (mechanism {this.mechanism}, algorithm {this.macAlgorithm.AlgorithmName})";
    }

    private SecretKeyObject CheckKey(KeyObject keyObject)
    {
        if (keyObject is SecretKeyObject key)
        {
            if (keyObject.CkaKeyType != CKK.CKK_GENERIC_SECRET)
            {
                if (this.addAllowedKeyType.HasValue)
                {
                    if (keyObject.CkaKeyType != this.addAllowedKeyType.Value)
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

            byte[] secretKey = key.GetSecret();
            if (mechanismInfo.MinKeySize > secretKey.Length || mechanismInfo.MaxKeySize < secretKey.Length)
            {
                throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
                    $"Mechanism {this.mechanism} require key size between {mechanismInfo.MinKeySize} and {mechanismInfo.MaxKeySize}. Key object {key.Id} has length {secretKey.Length}.");
            }

            return key;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required  Secret key.");
        }
    }
}