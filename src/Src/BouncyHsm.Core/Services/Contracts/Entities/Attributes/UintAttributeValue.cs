using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class UintAttributeValue : IAttributeValue
{
    private readonly uint value;

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.CkUint;
    }

    public UintAttributeValue(uint value)
    {
        this.value = value;
    }

    public bool AsBool()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkUint);
    }

    public byte[] AsByteArray()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkUint);
    }

    public string AsString()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkUint);
    }

    public uint AsUint()
    {
        return value;
    }

    public CkDate AsDate()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkUint);
    }

    public override string ToString()
    {
        return $"{GetType().Name}: {value}";
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || other.TypeTag != AttrTypeTag.CkUint)
        {
            return false;
        }

        return value == other.AsUint();
    }

    public bool Equals(uint other)
    {
        return value == other;
    }

    public uint GuessSize()
    {
        return 4;
    }
}
