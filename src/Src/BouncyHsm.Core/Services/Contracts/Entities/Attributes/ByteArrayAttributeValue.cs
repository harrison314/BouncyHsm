using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class ByteArrayAttributeValue : IAttributeValue
{
    public static readonly IAttributeValue Empty = new ByteArrayAttributeValue(Array.Empty<byte>());

    private readonly byte[] value;

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.ByteArray;
    }

    public ByteArrayAttributeValue(byte[] value)
    {
        this.value = value;
    }

    public bool AsBool()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.ByteArray);
    }

    public byte[] AsByteArray()
    {
        return value;
    }

    public string AsString()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.ByteArray);
    }

    public uint AsUint()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.ByteArray);
    }

    public CkDate AsDate()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.ByteArray);
    }

    public override string ToString()
    {
        return $"{GetType().Name}: {BitConverter.ToString(value)}";
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || other.TypeTag != AttrTypeTag.ByteArray)
        {
            return false;
        }

        return value.SequenceEqual(other.AsByteArray());
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return (uint)value.Length;
    }
}
