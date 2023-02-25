using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using BouncyHsm.Core.Services.Contracts.Entities;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class RsaWrapperSigner : IWrapperSigner
{
    private readonly ISigner signer;
    private readonly CKM mechanism;
    private readonly ILogger<RsaWrapperSigner> logger;

    public RsaWrapperSigner(ISigner signer, CKM mechanism, ILogger<RsaWrapperSigner> logger)
    {
        this.signer = signer;
        this.mechanism = mechanism;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to IntoSigningSigner.");

        if (keyObject is RsaPrivateKeyObject rsaPrivateKeyObject)
        {
            if (!rsaPrivateKeyObject.CkaSign)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", rsaPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
            }

            this.signer.Init(true, rsaPrivateKeyObject.GetPrivateKey());

            return new AuthenticatedSigner(this.signer, rsaPrivateKeyObject.CkaAlwaysAuthenticate);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required private RSA key.");
        }
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is RsaPublicKeyObject rsaPublicKeyObject)
        {
            if (!rsaPublicKeyObject.CkaVerify)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", rsaPublicKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
            }

            this.signer.Init(false, rsaPublicKeyObject.GetPublicKey());

            return this.signer;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required public RSA key.");
        }
    }

    public override string ToString()
    {
        return $"RsaWrapperSigner (mechanism {this.mechanism}, algorithm {this.signer.AlgorithmName})";
    }
}
