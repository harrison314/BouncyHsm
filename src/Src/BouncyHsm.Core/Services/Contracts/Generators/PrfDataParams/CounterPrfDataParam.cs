using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using System.Buffers.Binary;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal class CounterPrfDataParam : IPrfDataParam
{
    private readonly bool littleEndian;
    private readonly int width;

    public CK_PRF_DATA_TYPE Type
    {
        get => CK_PRF_DATA_TYPE.CK_SP800_108_COUNTER;
    }

    public CounterPrfDataParam(bool littleEndian, int widthInBits)
    {
        if (widthInBits % 8 != 0 || widthInBits < 8 || widthInBits > 64)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Invalid field widthInBits for CK_SP800_108_COUNTER, supported value has 8, 16, 32, 40, 48, 56, 64 actual is {widthInBits}.");
        }

        this.littleEndian = littleEndian;
        this.width = widthInBits / 8;
    }

    public void Apply(IMac prfFunction, ref PrfDataContext context)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ulong value = Convert.ToUInt64(context.Counter);

        if (this.littleEndian)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            prfFunction.BlockUpdate(buffer[..this.width]);
        }
        else
        {
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            prfFunction.BlockUpdate(buffer[^this.width..]);
        }
    }
}
