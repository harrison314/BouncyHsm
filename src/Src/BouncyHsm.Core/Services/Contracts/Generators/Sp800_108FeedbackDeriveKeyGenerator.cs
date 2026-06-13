using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class Sp800_108FeedbackDeriveKeyGenerator : Sp800_108DeriveKeyGenerator
{
    private readonly byte[] iv;

    public Sp800_108FeedbackDeriveKeyGenerator(CKM kdfMechanism, IPrfDataParam[] dataParams, byte[] iv, ILogger<Sp800_108FeedbackDeriveKeyGenerator> logger)
        : base(kdfMechanism, dataParams, logger)
    {
        this.iv = iv;
    }

    protected override void CheckDataParams(IPrfDataParam[] dataParams)
    {
        this.logger.LogTrace("Entering to CheckDataParams with dataParams count {DataParamsCount}", dataParams.Length);

        if (!dataParams.Any(t => t.Type == CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
              $"Mechanism {CKM.CKM_SP800_108_COUNTER_KDF} required data param of type {CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE}.");
        }
    }

    protected override byte[] DriveKey(byte[] keyValue, int requestedValueLen)
    {
        this.logger.LogTrace("Entering to DriveKey with requestedValueLen {RequestedValueLen}", requestedValueLen);

        Sp800_108FeedbackKdf kdf = new Sp800_108FeedbackKdf(() => MacUtils.TryGetPrf(this.KdfMechanism)!,
            keyValue,
            this.iv);

        return kdf.Derive(requestedValueLen, this.DataParams);
    }
}