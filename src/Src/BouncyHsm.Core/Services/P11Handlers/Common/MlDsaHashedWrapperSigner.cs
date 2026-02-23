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

internal class MlDsaHashedWrapperSigner : IWrapperSigner
{
    private readonly CKM mechanism;
    private readonly Ckp_CkSignAdditionalContext? mechanismParams;
    private readonly IDigest digest;
    private readonly ILogger<MlDsaHashedWrapperSigner> logger;

    public MlDsaHashedWrapperSigner(CKM mechanism,
        Ckp_CkSignAdditionalContext? mechanismParams,
        IDigest digest,
        ILogger<MlDsaHashedWrapperSigner> logger)
    {
        this.mechanism = mechanism;
        this.mechanismParams = mechanismParams;
        this.digest = digest;
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
            ISigner signer = HashMLDsaSignerFactory.Create(mlDsaPrivateKeyObject.CkaParameterSet,
                isDeterministic,
                this.digest);

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
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required private ML-DSA key.");
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

            bool isDeterministic = this.IsDeterministicRequired();
            ISigner signer = HashMLDsaSignerFactory.Create(mlDsaPublickeyObject.CkaParameterSet,
               isDeterministic,
               this.digest);

            if (this.mechanismParams?.Context != null)
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
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {this.mechanism} required public ML-DSA key.");
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
