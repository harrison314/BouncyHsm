using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities;
public abstract class KeyObject : StorageObject
{
    public CKK CkaKeyType
    {
        get => (CKK)this.values[CKA.CKA_KEY_TYPE].AsUint();
        set => this.values[CKA.CKA_KEY_TYPE] = AttributeValue.Create((uint)value);
    }

    public byte[] CkaId
    {
        get => this.values[CKA.CKA_ID].AsByteArray();
        set => this.values[CKA.CKA_ID] = AttributeValue.Create(value);
    }

    public CkDate CkaStartDate
    {
        get => this.values[CKA.CKA_START_DATE].AsDate();
        set => this.values[CKA.CKA_START_DATE] = AttributeValue.Create(value);
    }

    public CkDate CkaEndDate
    {
        get => this.values[CKA.CKA_END_DATE].AsDate();
        set => this.values[CKA.CKA_END_DATE] = AttributeValue.Create(value);
    }

    public bool CkaDerive
    {
        get => this.values[CKA.CKA_DERIVE].AsBool();
        set => this.values[CKA.CKA_DERIVE] = AttributeValue.Create(value);
    }

    public bool CkaLocal
    {
        get => this.values[CKA.CKA_LOCAL].AsBool();
        set => this.values[CKA.CKA_LOCAL] = AttributeValue.Create(value);
    }

    public CKM CkaKeyGenMechanism
    {
        get => (CKM)this.values[CKA.CKA_KEY_GEN_MECHANISM].AsUint();
        set => this.values[CKA.CKA_KEY_GEN_MECHANISM] = AttributeValue.Create((uint)value);
    }

    //TODO: Implement uint array attribute
    //public CKM[] AllovedMechanism
    //{
    //    get => (CKM)this.values[CKA.CKA_ALLOWED_MECHANISMS].AsUint();
    //    set => this.values[CKA.CKA_ALLOWED_MECHANISMS] = AttributeValue.Create((uint)value);
    //}

    public KeyObject(CKK keyType, CKM genMechanism)
    {
        this.CkaKeyType = keyType;
        this.CkaId = Array.Empty<byte>();
        this.CkaDerive = false;
        this.CkaLocal = true;
        this.CkaKeyGenMechanism = genMechanism;
        this.CkaStartDate = new CkDate();
        this.CkaEndDate = new CkDate();
    }

    internal KeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void Validate()
    {
        base.Validate();
        CryptoObjectValueChecker.CheckStartEndDate(this.CkaStartDate, this.CkaEndDate);
    }
}
