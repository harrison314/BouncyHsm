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

internal class SlhDsaWrapperSigner : IWrapperSigner
{
    private readonly Ckp_CkSignAdditionalContext? mechanismParams;
    private readonly ILogger<SlhDsaWrapperSigner> logger;

    public SlhDsaWrapperSigner(Ckp_CkSignAdditionalContext? mechanismParams, ILogger<SlhDsaWrapperSigner> logger)
    {
        this.mechanismParams = mechanismParams;
        this.logger = logger;
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to IntoSigningSigner.");

        if (keyObject is SlhDsaPrivateKeyObject slhDsaPrivateKeyObject)
        {
            if (!slhDsaPrivateKeyObject.CkaSign)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", slhDsaPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because objet is not authorized to sign (CKA_SIGN must by true).");
            }

            SlhDsaParameters parameters = SlhDsaUtils.GetParametersFromType(slhDsaPrivateKeyObject.CkaParameterSet);
            bool isDeterministic = this.IsDeterministicRequired();
            SlhDsaSigner signer = new SlhDsaSigner(parameters, isDeterministic);

            ICipherParameters cipherParameters = slhDsaPrivateKeyObject.GetPrivateKey();
            if (!isDeterministic)
            {
                cipherParameters = new ParametersWithRandom(cipherParameters, secureRandom);
                this.logger.LogDebug("Using non-deterministic SLH-DSA signer.");
            }

            if (this.mechanismParams?.Context != null)
            {
                cipherParameters = new ParametersWithContext(cipherParameters,
                    this.mechanismParams.Context);
                this.logger.LogDebug("Using additional context for SLH-DSA signer.");
            }

            signer.Init(true, cipherParameters);

            return new AuthenticatedSigner(signer, slhDsaPrivateKeyObject.CkaAlwaysAuthenticate);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_SLH_DSA} required private SLH-DSA private key.");
        }
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is SlhDsaPublicKeyObject slhDsaPublickeyObject)
        {
            if (!slhDsaPublickeyObject.CkaVerify)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", slhDsaPublickeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature operation is not allowed because objet is not authorized to verify (CKA_VERIFY must by true).");
            }

            SlhDsaParameters parameters = SlhDsaUtils.GetParametersFromType(slhDsaPublickeyObject.CkaParameterSet);
            SlhDsaSigner signer = new SlhDsaSigner(parameters, this.IsDeterministicRequired());

            if (this.mechanismParams?.Context != null)
            {
                signer.Init(false, new ParametersWithContext(slhDsaPublickeyObject.GetPublicKey(),
                    this.mechanismParams.Context));
            }
            else
            {
                signer.Init(false, slhDsaPublickeyObject.GetPublicKey());
            }

            return signer;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_SLH_DSA} required public SLH-DSA key.");
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