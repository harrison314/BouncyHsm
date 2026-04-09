using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public abstract class KeyObject : StorageObject
{
    public virtual CKK CkaKeyType
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

    public CKM[] AllovedMechanism
    {
        get => this.ConvertArray(this.values[CKA.CKA_ALLOWED_MECHANISMS].AsUintArray());
        set => this.values[CKA.CKA_ALLOWED_MECHANISMS] = AttributeValue.Create(this.ConvertArray(value));
    }

    public KeyObject(CKK keyType, CKM genMechanism)
    {
        this.CkaKeyType = keyType;
        this.CkaId = Array.Empty<byte>();
        this.CkaDerive = false;
        this.CkaLocal = true;
        this.CkaKeyGenMechanism = genMechanism;
        this.CkaStartDate = new CkDate();
        this.CkaEndDate = new CkDate();
        this.AllovedMechanism = this.GetAllovedMechanism();
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

    protected abstract CKM[] GetAllovedMechanism();

    private CKM[] ConvertArray(uint[] array)
    {
        if (array.Length == 0)
        {
            return Array.Empty<CKM>();
        }

        CKM[] destination = new CKM[array.Length];
        for (uint i = 0; i < array.Length; i++)
        {
            destination[i] = (CKM)array[i];
        }

        return destination;
    }

    private uint[] ConvertArray(CKM[] array)
    {
        if (array.Length == 0)
        {
            return Array.Empty<uint>();
        }

        uint[] destination = new uint[array.Length];
        for (uint i = 0; i < array.Length; i++)
        {
            destination[i] = (uint)array[i];
        }

        return destination;
    }
}
