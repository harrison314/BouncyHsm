using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using System.Buffers.Binary;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal class IterationVariablePrfDataParam : IPrfDataParam
{
    private readonly bool littleEndian;
    private readonly int width;

    public CK_PRF_DATA_TYPE Type
    {
        get => CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE;
    }

    public IterationVariablePrfDataParam(bool littleEndian, int widthInBits)
    {
        if (widthInBits % 8 != 0)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Invalid field widthInBits for CK_SP800_108_ITERATION_VARIABLE, widthInBits in BouncyHsm must be a multiple of eight bits, actual value is {widthInBits}.");
        }

        this.littleEndian = littleEndian;
        this.width = widthInBits / 8;
    }

    public void Apply(IMac prfFunction, ref PrfDataContext context)
    {
        if (context.AlternativeIteration != null)
        {
            this.ApplyAlternativeIteration(prfFunction, context.AlternativeIteration);
        }
        else
        {
            this.ApplyCounterIteration(prfFunction, context.Counter);
        }
    }

    private void ApplyAlternativeIteration(IMac prfFunction, byte[] bytes)
    {
        if (bytes.Length == this.width)
        {
            prfFunction.BlockUpdate(bytes, 0, bytes.Length);
            return;
        }

        if (this.littleEndian)
        {
            if (bytes.Length > this.width)
            {
                prfFunction.BlockUpdate(bytes[..this.width]);
            }
            else
            {
                prfFunction.BlockUpdate(bytes, 0, bytes.Length);
                int count = this.width - bytes.Length;
                for (int i = 0; i < count; i++)
                {
                    prfFunction.Update(0x00);
                }
            }
        }
        else
        {
            if (bytes.Length > this.width)
            {
                prfFunction.BlockUpdate(bytes[^this.width..]);
            }
            else
            {
                int count = this.width - bytes.Length;
                for (int i = 0; i < count; i++)
                {
                    prfFunction.Update(0x00);
                }

                prfFunction.BlockUpdate(bytes, 0, bytes.Length);
            }
        }
    }

    private void ApplyCounterIteration(IMac prfFunction, int counter)
    {
        if (this.width < 1 || this.width > 8)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Invalid field widthInBits for CK_SP800_108_ITERATION_VARIABLE, supported value has 8, 16, 32, 40, 48, 56, 64 actual is {this.width * 8}.");
        }

        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ulong value = Convert.ToUInt64(counter);

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
