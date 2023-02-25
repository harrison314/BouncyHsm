using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using BouncyHsm.Core.Services.Contracts.Entities;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class EcdsaWrapperSigner : IWrapperSigner
{
    private readonly ISigner signer;
    private readonly CKM mechanism;
    private readonly ILogger<EcdsaWrapperSigner> logger;

    public EcdsaWrapperSigner(ISigner signer, CKM mechanism, ILogger<EcdsaWrapperSigner> logger)
    {
        this.signer = signer;
        this.mechanism = mechanism;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to IntoSigningSigner.");

        if (keyObject is EcdsaPrivateKeyObject ecPrivateKeyObject)
        {
            if (!ecPrivateKeyObject.CkaSign)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", ecPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
            }

            this.signer.Init(true, ecPrivateKeyObject.GetPrivateKey());

            return new AuthenticatedSigner(this.signer, ecPrivateKeyObject.CkaAlwaysAuthenticate);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required private EC key.");
        }
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is EcdsaPublicKeyObject ecPublicKeyObject)
        {
            if (!ecPublicKeyObject.CkaVerify)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", ecPublicKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
            }

            this.signer.Init(false, ecPublicKeyObject.GetPublicKey());

            return this.signer;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required public RSA key.");
        }
    }

    public override string ToString()
    {
        return $"EcdsaWrapperSigner (mechanism {this.mechanism}, algorithm {this.signer.AlgorithmName})";
    }
}