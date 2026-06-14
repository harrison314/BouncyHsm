using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class Sp800_108CounterDeriveKeyGenerator : Sp800_108DeriveKeyGenerator
{
    public Sp800_108CounterDeriveKeyGenerator(CKM kdfMechanism, IPrfDataParam[] dataParams, ILogger<Sp800_108CounterDeriveKeyGenerator> logger)
        : base(kdfMechanism, dataParams, logger)
    {
    }

    protected override void CheckDataParams(IPrfDataParam[] dataParams)
    {
        this.logger.LogTrace("Entering to CheckDataParams with dataParams count {DataParamsCount}", dataParams.Length);

        if (!dataParams.Any(t => t.Type == CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
              $"Mechanism {CKM.CKM_SP800_108_COUNTER_KDF} required data param of type {CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE}.");
        }

        if (dataParams.Any(t => t.Type == CK_PRF_DATA_TYPE.CK_SP800_108_COUNTER))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
              $"For mechanism {CKM.CKM_SP800_108_COUNTER_KDF} data param of type {CK_PRF_DATA_TYPE.CK_SP800_108_COUNTER} is invalid.");
        }
    }

    protected override byte[] DriveKey(byte[] keyValue, int requestedValueLen)
    {
        this.logger.LogTrace("Entering to DriveKey with requestedValueLen {RequestedValueLen}", requestedValueLen);

        Sp800_108CounterKdf kdf = new Sp800_108CounterKdf(() => MacUtils.TryGetPrf(this.KdfMechanism)!, keyValue);
        return kdf.Derive(requestedValueLen, this.DataParams);
    }

    public override string ToString()
    {
        return $"Sp800_108CounterDeriveKeyGenerator with KDF {this.KdfMechanism}";
    }
}