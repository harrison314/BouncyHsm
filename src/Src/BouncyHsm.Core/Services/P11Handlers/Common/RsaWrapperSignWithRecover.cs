using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts.Entities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class RsaWrapperSignWithRecover : IWrapperSignWithRecover
{
    private readonly ISignerWithRecovery signer;
    private readonly CKM mechanism;
    private readonly ILogger<RsaWrapperSignWithRecover> logger;

    public RsaWrapperSignWithRecover(ISignerWithRecovery signer, CKM mechanism, ILogger<RsaWrapperSignWithRecover> logger)
    {
        this.signer = signer;
        this.mechanism = mechanism;
        this.logger = logger;
    }

    public AuthenticatedSignerWithRecovery IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to IntoSigningSigner.");

        if (keyObject is RsaPrivateKeyObject rsaPrivateKeyObject)
        {
            if (!rsaPrivateKeyObject.CkaSignRecover)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN_RECOVER to true.", rsaPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN_RECOVER must by true).");
            }

            this.signer.Init(true, rsaPrivateKeyObject.GetPrivateKey());

            return new AuthenticatedSignerWithRecovery(this.signer, rsaPrivateKeyObject.CkaAlwaysAuthenticate);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required private RSA key.");
        }
    }

    public ISignerWithRecovery IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is RsaPublicKeyObject rsaPublicKeyObject)
        {
            if (!rsaPublicKeyObject.CkaVerifyRecover)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY_RECOVER to true.", rsaPublicKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature with recover operation is not allowed because objet is not authorized to verify (CKA_VERIFY_RECOVER must by true).");
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
        return $"RsaWrapperSignWithRecover (mechanism {this.mechanism}, algorithm {this.signer.AlgorithmName})";
    }
}