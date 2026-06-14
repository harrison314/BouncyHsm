using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using System.Buffers.Binary;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal class DkmLengthPrfDataParam : IPrfDataParam
{
    private readonly bool littleEndian;
    private readonly int widthInBits;
    private readonly CK_SP800_108_DKM_LENGTH_METHOD lengthMethod;
    private readonly int width;

    public CK_PRF_DATA_TYPE Type
    {
        get => CK_PRF_DATA_TYPE.CK_SP800_108_DKM_LENGTH;
    }

    public DkmLengthPrfDataParam(bool littleEndian, int widthInBits, CK_SP800_108_DKM_LENGTH_METHOD lengthMethod)
    {
        if (widthInBits % 8 != 0 || widthInBits < 8 || widthInBits > 64)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Invalid field widthInBits for CK_SP800_108_DKM_LENGTH, supported value has 8, 16, 32, 40, 48, 56, 64 actual is {widthInBits}.");
        }

        this.littleEndian = littleEndian;
        this.widthInBits = widthInBits;
        this.lengthMethod = lengthMethod;
        this.width = widthInBits / 8;
    }

    public void Apply(IMac prfFunction, ref PrfDataContext context)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ulong value = this.lengthMethod switch
        {
            CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS => Convert.ToUInt64(context.DkmLenghth * 8),
            CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_SEGMENTS => Convert.ToUInt64(context.BlockTotalLength * 8),
            _ => throw new InvalidProgramException($"Enum value CK_SP800_108_DKM_LENGTH_METHOD is not supported.")
        };

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

    public override string ToString()
    {
        return $"Prf data: {this.Type} with littleEndian {this.littleEndian}, width: {this.width}B, lengthMethod {this.lengthMethod}";
    }
}