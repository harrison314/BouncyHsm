using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class CkDateAttributeValue : IAttributeValue
{
    public static readonly IAttributeValue Empty = new CkDateAttributeValue(new CkDate());

    private readonly CkDate date;

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.DateTime;
    }

    public CkDateAttributeValue(CkDate date)
    {
        this.date = date;
    }

    public bool AsBool()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.DateTime);
    }

    public byte[] AsByteArray()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.DateTime);
    }

    public string AsString()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.DateTime);
    }

    public uint AsUint()
    {
        throw new InvalidATtributeTypeCastException(AttrTypeTag.DateTime);
    }

    public CkDate AsDate()
    {
        return date;
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.TypeTag == AttrTypeTag.DateTime)
        {

        }

        return false;
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return date.HasValue ? 8U : 0U;
    }

    public override string ToString()
    {
        return $"{GetType().Name}: {date}";
    }
}