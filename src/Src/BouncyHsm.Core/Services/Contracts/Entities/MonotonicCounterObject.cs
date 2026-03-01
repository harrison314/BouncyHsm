using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class MonotonicCounterObject : IHardwareFeature
{
    public const uint HwHandle = 52;

    private readonly IPersistentRepository persistentRepository;
    private readonly uint slotId;

    public CKO CkaClass
    {
        get => CKO.CKO_HW_FEATURE;
    }


    public CKH CkHwFeatureType
    {
        get => CKH.CKH_MONOTONIC_COUNTER;
    }
    
    public bool ResetOnInit
    {
        get => true;
    }

    public bool HasReset
    {
        get;
        private set;
    }

    public byte[] Value
    {
        get;
        private set;
    }

    private MonotonicCounterObject(IPersistentRepository persistentRepository, uint slotId)
    {
        this.persistentRepository = persistentRepository;
        this.slotId = slotId;
        this.Value = new byte[sizeof(ulong)];
    }

    public static async Task<MonotonicCounterObject> Load(IPersistentRepository persistentRepository, uint slotId)
    {
        MonotonicCounterObject self = new MonotonicCounterObject(persistentRepository, slotId);
        SlotEntity slot = await self.persistentRepository.EnsureSlot(self.slotId, true, default);
        BinaryPrimitives.WriteUInt64BigEndian(self.Value.AsSpan(), slot.Token.MonotonicCounter);
        self.HasReset = slot.Token.MonotonicCounterHasReset;


        return self;
    }

    public void Accept(ICryptoApiObjectVisitor visitor)
    {
        throw new NotImplementedException();
    }

    public T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public AttributeValueResult GetValue(CKA attributeType, CryptoApiObjectGetValueMode mode)
    {
        return attributeType switch
        {
            CKA.CKA_CLASS => new AttributeValueResult.Ok(AttributeValue.Create((uint)this.CkaClass)),
            CKA.CKA_HW_FEATURE_TYPE => new AttributeValueResult.Ok(AttributeValue.Create((uint)this.CkHwFeatureType)),
            CKA.CKA_RESET_ON_INIT => new AttributeValueResult.Ok(AttributeValue.Create(this.ResetOnInit)),
            CKA.CKA_HAS_RESET => new AttributeValueResult.Ok(AttributeValue.Create(this.HasReset)),
            CKA.CKA_VALUE when mode == CryptoApiObjectGetValueMode.SkipComputing => new AttributeValueResult.Ok(AttributeValue.Create(this.Value)),
            CKA.CKA_VALUE when mode == CryptoApiObjectGetValueMode.Default => new AttributeValueResult.Computed(this.IncerementValue(), true),
            _ => new AttributeValueResult.InvalidAttribute()
        };
    }

    public bool IsMatch(IEnumerable<KeyValuePair<CKA, IAttributeValue>> matchTemplate)
    {
        foreach ((CKA attrType, IAttributeValue attrValue) in matchTemplate)
        {
            if (attrType == CKA.CKA_CLASS && attrValue.Equals((uint)this.CkaClass))
            {
                continue;
            }

            if (attrType == CKA.CKA_HW_FEATURE_TYPE && attrValue.Equals((uint)this.CkHwFeatureType))
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

    private async Task<IAttributeValue> IncerementValue()
    {
        UpdateSlotCommand command = new UpdateSlotCommand();

        await this.persistentRepository.ExecuteSlotCommand(this.slotId, command, default);

        byte[] buffer = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(), command.MonotonicCounter);
        return AttributeValue.Create(buffer);
    }

    private class UpdateSlotCommand : IPersistentRepositorySlotCommand
    {
        public ulong MonotonicCounter
        {
            get;
            private set;
        }

        public UpdateSlotCommand()
        {

        }

        public bool UpdateSlot(SlotEntity slotEntity)
        {
            slotEntity.Token.MonotonicCounter++;
            this.MonotonicCounter = slotEntity.Token.MonotonicCounter;
            return true;
        }
    }
}