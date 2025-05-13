using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class EdDsaCipherWrapper : IWrapperSigner
{
    private readonly Ckp_CkEddsaParams? eddsaParams;
    private readonly ILogger<EdDsaCipherWrapper> logger;

    public EdDsaCipherWrapper(Ckp_CkEddsaParams? eddsaParams, ILogger<EdDsaCipherWrapper> logger)
    {
        this.eddsaParams = eddsaParams;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to IntoSigningSigner.");
        if (keyObject is EdwardsPrivateKeyObject edwardsKey)
        {
            if (!edwardsKey.CkaSign)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", edwardsKey.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
            }
            AsymmetricKeyParameter privateKey = edwardsKey.GetPrivateKey();
            ISigner signer = privateKey switch
            {
                Ed25519PrivateKeyParameters => this.CreateEd25519Signer(),
                Ed448PrivateKeyParameters => this.CreateEd448Signer(),
                _ => throw new InvalidParameterException($"Private key type {privateKey.GetType().FullName} is not supported.")
            };

            signer.Init(true, privateKey);

            return new AuthenticatedSigner(signer, edwardsKey.CkaAlwaysAuthenticate);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_EDDSA} required private Edwards key.");
        }
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is EdwardsPublicKeyObject edwardsKey)
        {
            if (!edwardsKey.CkaVerify)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", edwardsKey.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
            }

            AsymmetricKeyParameter publicKey = edwardsKey.GetPublicKey();
            ISigner signer = publicKey switch
            {
                Ed25519PublicKeyParameters => this.CreateEd25519Signer(),
                Ed448PublicKeyParameters => this.CreateEd448Signer(),
                _ => throw new InvalidParameterException($"Public key type {publicKey.GetType().FullName} is not supported.")
            };

            signer.Init(false, publicKey);

            return signer;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_EDDSA} required public Edwards key.");
        }
    }

    private ISigner CreateEd25519Signer()
    {
        if (this.eddsaParams == null)
        {
            this.logger.LogTrace("Creating Ed25519Signer.");
            return new Ed25519Signer();
        }

        if (this.eddsaParams.PhFlag)
        {
            this.logger.LogTrace("Creating Ed25519phSigner.");
            return new Ed25519phSigner(this.eddsaParams.ContextData?? Array.Empty<byte>());
        }
        else
        {
            this.logger.LogTrace("Creating Ed25519ctxSigner.");
            return new Ed25519ctxSigner(this.eddsaParams.ContextData ?? Array.Empty<byte>());
        }
    }

    private ISigner CreateEd448Signer()
    {
        if (this.eddsaParams == null)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, 
                "For ED448 Edwards key is parameters CK_EDDSA_PARAMS required.");
        }

        if (this.eddsaParams.PhFlag)
        {
            this.logger.LogTrace("Creating Ed448phSigner.");
            return new Ed448phSigner(this.eddsaParams.ContextData ?? Array.Empty<byte>());
        }
        else
        {
            this.logger.LogTrace("Creating Ed448Signer.");
            return new Ed448Signer(this.eddsaParams.ContextData ?? Array.Empty<byte>());
        }
    }

    public override string ToString()
    {
        return "EdDsaCipherWrapper";
    }
}
