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
        return this.value;
    }

    public CkDate AsDate()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkUint);
    }

    public override string ToString()
    {
        return $"{this.GetType().Name}: {this.value}";
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || other.TypeTag != AttrTypeTag.CkUint)
        {
            return false;
        }

        return this.value == other.AsUint();
    }

    public bool Equals(uint other)
    {
        return this.value == other;
    }

    public uint GuessSize()
    {
        return 4;
    }
}
