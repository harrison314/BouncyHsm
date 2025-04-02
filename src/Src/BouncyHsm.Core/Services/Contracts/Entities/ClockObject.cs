using BouncyHsm.Core.Services.Contracts.P11;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class ClockObject : IHardwareFeature
{
    private readonly ITimeAccessor timeAccessor;

    public const uint HwHandle = 51;

    public CKO CkaClass
    {
        get => CKO.CKO_HW_FEATURE;
    }

    public CKH CkHwFeatureType
    {
        get => CKH.CKH_CLOCK;
    }

    public string Value
    {
        get => string.Format("{0:yyyyMMddHHmmss}00", this.timeAccessor.UtcNow);
    }

    public ClockObject(ITimeAccessor timeAccessor)
    {
        this.timeAccessor = timeAccessor;
    }

    public AttributeValueResult GetValue(CKA attributeType)
    {
        return attributeType switch
        {
            CKA.CKA_CLASS => new AttributeValueResult.Ok(AttributeValue.Create((uint)CKH.CKH_CLOCK)),
            CKA.CKA_HW_FEATURE_TYPE => new AttributeValueResult.Ok(AttributeValue.Create((uint)CKH.CKH_CLOCK)),
            CKA.CKA_VALUE => new AttributeValueResult.Ok(AttributeValue.Create(Encoding.UTF8.GetBytes(this.Value.PadRight(16, ' ')))),
            _ => new AttributeValueResult.InvalidAttribute()
        };
    }

    public bool IsMatch(IEnumerable<KeyValuePair<CKA, IAttributeValue>> matchTemplate)
    {
        foreach ((CKA attrType, IAttributeValue attrValue) in matchTemplate)
        {
            if (attrType == CKA.CKA_CLASS && attrValue.Equals((uint)CKO.CKO_HW_FEATURE))
            {
                continue;
            }

            if (attrType == CKA.CKA_HW_FEATURE_TYPE && attrValue.Equals((uint)CKH.CKH_CLOCK))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public uint? TryGetSize(bool isLoggedIn)
    {
        return 0;
    }

    public void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}
