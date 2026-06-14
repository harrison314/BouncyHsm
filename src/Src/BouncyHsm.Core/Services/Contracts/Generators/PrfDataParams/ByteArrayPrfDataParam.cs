using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal class ByteArrayPrfDataParam : IPrfDataParam
{
    private readonly byte[] data;

    public CK_PRF_DATA_TYPE Type
    {
        get => CK_PRF_DATA_TYPE.CK_SP800_108_BYTE_ARRAY;
    }

    public ByteArrayPrfDataParam(byte[] data)
    {
        this.data = data;
    }

    public void Apply(IMac prfFunction, ref PrfDataContext context)
    {
        prfFunction.BlockUpdate(this.data, 0, this.data.Length);
    }

    public override string ToString()
    {
        return $"Prf data: {this.Type} with array length {this.data.Length}";
    }
}
