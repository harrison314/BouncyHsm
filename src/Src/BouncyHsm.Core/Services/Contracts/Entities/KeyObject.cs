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

    public CKM[] CkaAllovedMechanism
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
        this.CkaAllovedMechanism = this.GetAllovedMechanism();
    }

    internal KeyObject(StorageObjectMemento memento)
        : base(memento)
    {

    }

    public override void Validate()
    {
        base.Validate();
        CryptoObjectValueChecker.CheckStartEndDate(this.CkaStartDate, this.CkaEndDate);
        CryptoObjectValueChecker.CheckAllovedMechanism(this.CkaAllovedMechanism, this.GetAllovedMechanism());
    }

    public override void MigrateObject(MigrateObjectFlags flags)
    {
        base.MigrateObject(flags);

        if (flags.HasFlag(MigrateObjectFlags.ResetAlowedMechanism))
        {
            this.CkaAllovedMechanism = this.GetAllovedMechanism();
        }
    }

    public bool MechanismIsAllowed(CKM mechanism)
    {
        return this.values[CKA.CKA_ALLOWED_MECHANISMS].AsUintArray().Contains((uint)mechanism);
    }

    protected abstract CKM[] GetAllovedMechanism();

    private CKM[] ConvertArray(uint[] array)
    {
        if (array.Length == 0)
        {
            return Array.Empty<CKM>();
        }

        CKM[] destination = new CKM[array.Length];

        System.Diagnostics.Debug.Assert(sizeof(uint) == sizeof(CKM));
        Buffer.BlockCopy(array, 0, destination, 0, destination.Length * sizeof(uint));

        return destination;
    }

    private uint[] ConvertArray(CKM[] array)
    {
        if (array.Length == 0)
        {
            return Array.Empty<uint>();
        }

        uint[] destination = new uint[array.Length];

        System.Diagnostics.Debug.Assert(sizeof(uint) == sizeof(CKM));
        Buffer.BlockCopy(array, 0, destination, 0, destination.Length * sizeof(uint));

        return destination;
    }
}
