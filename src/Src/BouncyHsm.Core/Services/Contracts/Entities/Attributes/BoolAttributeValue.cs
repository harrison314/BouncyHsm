using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class BoolAttributeValue : IAttributeValue
{
    public static readonly IAttributeValue True = new BoolAttributeValue(true);
    public static readonly IAttributeValue False = new BoolAttributeValue(false);

    private readonly bool value;

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.CkBool;
    }

    public BoolAttributeValue(bool value)
    {
        this.value = value;
    }

    public bool AsBool()
    {
        return value;
    }

    public byte[] AsByteArray()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkBool);
    }

    public string AsString()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkBool);
    }

    public uint AsUint()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkBool);
    }

    public CkDate AsDate()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.CkBool);
    }

    public override string ToString()
    {
        return $"{GetType().Name}: {value}";
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null || other.TypeTag != AttrTypeTag.CkBool)
        {
            return false;
        }

        return value == other.AsBool();
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return 1;
    }
}
