using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System.Buffers.Binary;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class MonotonicCounterObject : IHardwareFeature
{
    private readonly IPersistentRepository persistentRepository;
    private readonly uint slotId;
    private SlotEntity? slot;

    public CKH CkHwFeatureType
    {
        get => CKH.CKH_MONOTONIC_COUNTER;
    }

    public CKO CkaClass
    {
        get => CKO.CKO_HW_FEATURE;
    }

    public bool ResetOnInit
    {
        get => true;
    }

    public bool HasReset
    {
        get => true;
    }

    public byte[] Value
    {
        get => this.IncrementAndreturnValue();
    }

    public MonotonicCounterObject(IPersistentRepository persistentRepository, uint slotId)
    {
        this.persistentRepository = persistentRepository;
        this.slotId = slotId;
    }

    public void Accept(ICryptoApiObjectVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public AttributeValueResult GetValue(CKA attributeType)
    {
        return attributeType switch
        {
            CKA.CKA_CLASS => new AttributeValueResult.Ok(AttributeValue.Create((uint)CKH.CKH_MONOTONIC_COUNTER)),
            CKA.CKA_HW_FEATURE_TYPE => new AttributeValueResult.Ok(AttributeValue.Create((uint)CKH.CKH_CLOCK)),
            CKA.CKA_RESET_ON_INIT => new AttributeValueResult.Ok(AttributeValue.Create(this.ResetOnInit)),
            CKA.CKA_HAS_RESET => new AttributeValueResult.Ok(AttributeValue.Create(this.HasReset)),
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

            if (attrType == CKA.CKA_HW_FEATURE_TYPE && attrValue.Equals((uint)CKH.CKH_MONOTONIC_COUNTER))
            {
                continue;
            }

            if (attrType == CKA.CKA_RESET_ON_INIT && attrValue.Equals(this.ResetOnInit))
            {
                continue;
            }

            if (attrType == CKA.CKA_HAS_RESET && attrValue.Equals(this.HasReset))
            {
                continue;
            }

            if (attrType == CKA.CKA_VALUE && attrValue.Equals(this.Value))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public uint? TryGetSize(bool isLoggedIn)
    {
        return sizeof(ulong) + sizeof(int);
    }

    private byte[] IncrementAndreturnValue()
    {
        //TODO: Async all the way.
        SlotEntity? slot = this.persistentRepository.GetSlot(this.slotId, default).GetAwaiter().GetResult();
        
        byte[] buffer = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(), slot.Token.MonotonicCounter);
        return buffer;
    }

    private async Task<ulong> IncrementAndreturnValueAsync()
    {
        SlotEntity slot = await this.EnshureSlot();


        ulong value = 0;
        byte[] buffer = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(), value);
        return buffer;
    }

    private async Task<SlotEntity> EnshureSlot()
    {
        if (this.slot != null)
        {
            return this.slot;
        }

        this.slot = await this.persistentRepository.EnsureSlot(this.slotId, true, default);

        return this.slot;
    }
}