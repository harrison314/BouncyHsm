using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class Sp800_108DoublePipelineDeriveKeyGenerator : Sp800_108DeriveKeyGenerator
{
    public Sp800_108DoublePipelineDeriveKeyGenerator(CKM kdfMechanism, IPrfDataParam[] dataParams, ILogger<Sp800_108DoublePipelineDeriveKeyGenerator> logger)
        : base(kdfMechanism, dataParams, logger)
    {
    }

    protected override void CheckDataParams(IPrfDataParam[] dataParams)
    {
        if (!dataParams.Any(t => t.Type == CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
              $"Mechanism {CKM.CKM_SP800_108_COUNTER_KDF} required data param of type {CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE}.");
        }
    }

    protected override byte[] DriveKey(byte[] keyValue, int requestedValueLen)
    {
        Sp800_108DoublePipelineKdf kdf = new Sp800_108DoublePipelineKdf(() => MacUtils.TryGetPrf(this.KdfMechanism)!, keyValue);
        return kdf.Derive(requestedValueLen, this.DataParams);
    }
}