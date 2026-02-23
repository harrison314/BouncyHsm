using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class MlDsaPrehashedWrapperSigner : IWrapperSigner
{
    private readonly Ckp_CkHashSignAdditionalContext mechanismParams;
    private readonly ILogger<MlDsaPrehashedWrapperSigner> logger;

    public MlDsaPrehashedWrapperSigner(Ckp_CkHashSignAdditionalContext mechanismParams, ILogger<MlDsaPrehashedWrapperSigner> logger)
    {
        this.mechanismParams = mechanismParams;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to IntoSigningSigner.");

        if (keyObject is MlDsaPrivateKeyObject mlDsaPrivateKeyObject)
        {
            if (!mlDsaPrivateKeyObject.CkaSign)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", mlDsaPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
            }

            bool isDeterministic = this.IsDeterministicRequired();

            IDigest? digest = DigestUtils.TryGetDigest((CKM)this.mechanismParams.Hash);
            if (digest == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    $"Hash mechanism {this.mechanismParams.Hash} in param CkHashSignAdditionalContext is not supported for ML-DSA.");
            }

            ISigner signer = HashMLDsaSignerFactory.CreatePrehash(mlDsaPrivateKeyObject.CkaParameterSet,
               isDeterministic,
               digest);

            ICipherParameters cipherParameters = mlDsaPrivateKeyObject.GetPrivateKey();
            if (!isDeterministic)
            {
                cipherParameters = new ParametersWithRandom(cipherParameters, secureRandom);
                this.logger.LogDebug("Using non-deterministic ML-DSA signer.");
            }

            if (this.mechanismParams?.Context != null)
            {
                cipherParameters = new ParametersWithContext(cipherParameters,
                    this.mechanismParams.Context);
                this.logger.LogDebug("Using additional context for ML-DSA signer.");
            }

            signer.Init(true, cipherParameters);

            return new AuthenticatedSigner(signer, mlDsaPrivateKeyObject.CkaAlwaysAuthenticate);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_ML_DSA} required private ML-DSA key.");
        }
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is MlDsaPublicKeyObject mlDsaPublickeyObject)
        {
            if (!mlDsaPublickeyObject.CkaVerify)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", mlDsaPublickeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
            }

            IDigest? digest = DigestUtils.TryGetDigest((CKM)this.mechanismParams.Hash);
            if (digest == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    $"Hash mechanism {this.mechanismParams.Hash} in param CkHashSignAdditionalContext is not supported for ML-DSA.");
            }

            bool isDeterministic = this.IsDeterministicRequired();

            ISigner signer = HashMLDsaSignerFactory.CreatePrehash(mlDsaPublickeyObject.CkaParameterSet,
                isDeterministic,
                digest);

            if (this.mechanismParams.Context != null)
            {
                signer.Init(false, new ParametersWithContext(mlDsaPublickeyObject.GetPublicKey(),
                    this.mechanismParams.Context));
            }
            else
            {
                signer.Init(false, mlDsaPublickeyObject.GetPublicKey());
            }

            return signer;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_ML_DSA} required public ML-DSA key.");
        }
    }

    private bool IsDeterministicRequired()
    {
        const bool DefaultHedgeVariant = false;

        if (this.mechanismParams is null)
        {
            return DefaultHedgeVariant;
        }

        return ((CK_HEDGE_TYPE)this.mechanismParams.HedgeVariant) switch
        {
            CK_HEDGE_TYPE.CKH_DETERMINISTIC_REQUIRED => true,
            CK_HEDGE_TYPE.CKH_HEDGE_PREFERRED => DefaultHedgeVariant,
            CK_HEDGE_TYPE.CKH_HEDGE_REQUIRED => false,
            _ => throw new InvalidProgramException($"Enum value {this.mechanismParams.HedgeVariant} is not supported.")
        };
    }
}
