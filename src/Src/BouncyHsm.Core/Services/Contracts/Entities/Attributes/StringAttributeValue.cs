using BouncyHsm.Core.Services.Contracts.P11;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class StringAttributeValue : IAttributeValue
{
    private readonly string value;

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.String;
    }

    public StringAttributeValue(string value)
    {
        this.value = value;
    }

    public bool AsBool()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.String);
    }

    public byte[] AsByteArray()
    {
        return Encoding.UTF8.GetBytes(value);
    }

    public string AsString()
    {
        return value;
    }

    public uint AsUint()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.String);
    }

    public CkDate AsDate()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.String);
    }

    public override string ToString()
    {
        return $"{GetType().Name}: {value}";
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || other.TypeTag != AttrTypeTag.String)
        {
            return false;
        }

        return string.Equals(value, other.AsString(), StringComparison.Ordinal);
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return (uint)Encoding.UTF8.GetByteCount(value);
    }
}
