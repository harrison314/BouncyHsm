using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal class KeyHandlePrfDataParam : IPrfDataParam
{
    private readonly SecretKeyObject secretKey;

    public CK_PRF_DATA_TYPE Type
    {
        get => CK_PRF_DATA_TYPE.CK_SP800_108_KEY_HANDLE;
    }

    public KeyHandlePrfDataParam(SecretKeyObject secretKey)
    {
        this.secretKey = secretKey;
    }

    public void Apply(IMac prfFunction, ref PrfDataContext context)
    {
        byte[] data = this.secretKey.GetSecret();
        prfFunction.BlockUpdate(data, 0, data.Length);
    }
}