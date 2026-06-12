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
                $"Invalid filed widthInBits for CK_SP800_108_ITERATION_VARIABLE, supported value has asas TODO actual is {widthInBits}.");
        }

        this.littleEndian = littleEndian;
        this.width = widthInBits / 8;
    }

    public void Apply(IMac dataWriter, ref PrfDataContext context)
    {
        if (context.AlternativeIteration != null)
        {
            this.ApplyAlternativeIteration(dataWriter, context.AlternativeIteration);
        }
        else
        {
            this.ApplyCounterIteration(dataWriter, context.Counter);
        }
    }

    private void ApplyAlternativeIteration(IMac dataWriter, byte[] bytes)
    {
        if (bytes.Length == this.width)
        {
            dataWriter.BlockUpdate(bytes, 0, bytes.Length);
            return;
        }

        if (this.littleEndian)
        {
            if (bytes.Length > this.width)
            {
                dataWriter.BlockUpdate(bytes[this.width..]);
            }
            else
            {
                dataWriter.BlockUpdate(bytes, 0, bytes.Length);
                int count = this.width - bytes.Length;
                for (int i = 0; i < count; i++)
                {
                    dataWriter.Update(0x00);
                }
            }
        }
        else
        {
            if (bytes.Length > this.width)
            {
                dataWriter.BlockUpdate(bytes[^this.width..]);
            }
            else
            {
                int count = this.width - bytes.Length;
                for (int i = 0; i < count; i++)
                {
                    dataWriter.Update(0x00);
                }

                dataWriter.BlockUpdate(bytes, 0, bytes.Length);
            }
        }
    }

    private void ApplyCounterIteration(IMac dataWriter, int counter)
    {
        if (this.width < 1 || this.width > 8)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Invalid filed widthInBits for CK_SP800_108_ITERATION_VARIABLE, supported value has 8, 16, 32, 40, 48, 56, 64 actual is {widthInBits}.");
        }

        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        ulong value = Convert.ToUInt64(counter);

        if (this.littleEndian)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
            dataWriter.BlockUpdate(buffer[this.width..]);
        }
        else
        {
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
            dataWriter.BlockUpdate(buffer[^this.width..]);
        }
    }
}
