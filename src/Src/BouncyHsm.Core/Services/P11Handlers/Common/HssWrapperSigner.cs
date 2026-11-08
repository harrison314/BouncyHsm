using BouncyHsm.Core.Services.Bc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class HssWrapperSigner : IStatefulWrapperSigner
{
    private readonly ILogger<HssWrapperSigner> logger;

    public bool AcceptEmptyData
    {
        get => true;
    }

    public HssWrapperSigner(ILogger<HssWrapperSigner> logger)
    {
        this.logger = logger;
    }

    public IStatefulSigner IntoStatefulSigner(KeyObject keyObject, IP11HwServices hwServices, IP11Session session)
    {
        this.logger.LogTrace("Entering to IntoStatefulSigner.");

        if (keyObject is HssPrivateKeyObject hssPrivateKeyObject)
        {
            if (!hssPrivateKeyObject.CkaSign)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_SIGN to true.", hssPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The signature operation is not allowed because object is not authorized to sign (CKA_SIGN must be true).");
            }

            if (hssPrivateKeyObject.CkaHssKeysRemaining == 0)
            {
                this.logger.LogError("HSS private key with id {ObjectId} is exhausted.", hssPrivateKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_EXHAUSTED, "HSS private key is exhausted.");
            }

            return new HssStatefulSigner(hwServices,
                session,
                hssPrivateKeyObject.Id,
                hssPrivateKeyObject.CkaToken,
                HssUtils.GetSignatureLength(hssPrivateKeyObject.CkaHssLmsTypes, hssPrivateKeyObject.CkaHssLmotsTypes),
                this.logger);
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_HSS} required HSS private key.");
        }
    }

    public AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom)
    {
        throw new InvalidOperationException($"Mechanism {CKM.CKM_HSS} is stateful, use IntoStatefulSigner.");
    }

    public ISigner IntoValidationSigner(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to IntoValidationSigner.");

        if (keyObject is HssPublicKeyObject hssPublicKeyObject)
        {
            if (!hssPublicKeyObject.CkaVerify)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_VERIFY to true.", hssPublicKeyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The verification signature operation is not allowed because object is not authorized to verify (CKA_VERIFY must be true).");
            }

            MessageSignerAdapter signer = new MessageSignerAdapter(new HssSigner(), "HSS");
            signer.Init(false, hssPublicKeyObject.GetPublicKey());

            return signer;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"Mechanism {CKM.CKM_HSS} required HSS public key.");
        }
    }
}
