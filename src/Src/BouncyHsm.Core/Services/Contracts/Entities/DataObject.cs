using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public sealed class DataObject : StorageObject
{
    public override CKO CkaClass
    {
        get => CKO.CKO_DATA;
    }

    /// <summary>
    /// RFC2279 string
    /// </summary>
    public string CkaApplication
    {
        get => this.values[CKA.CKA_APPLICATION].AsString();
        set => this.values[CKA.CKA_APPLICATION] = AttributeValue.Create(value);
    }

    public byte[] CkaObjectId
    {
        get => this.values[CKA.CKA_OBJECT_ID].AsByteArray();
        set => this.values[CKA.CKA_OBJECT_ID] = AttributeValue.Create(value);
    }

    public byte[] CkaValue
    {
        get => this.values[CKA.CKA_VALUE].AsByteArray();
        set => this.values[CKA.CKA_VALUE] = AttributeValue.Create(value);
    }

    public DataObject()
    {
        this.CkaApplication = string.Empty;
        this.CkaObjectId = Array.Empty<byte>();
        this.CkaValue = Array.Empty<byte>();
    }

    internal DataObject(StorageObjectMemento memento)
        :base(memento)
    {
        
    }

    public override void Validate()
    {
        CryptoObjectValueChecker.CheckDerObjectIdentifier(CKA.CKA_OBJECT_ID, this.CkaObjectId, true);
    }

    public override void Accept(ICryptoApiObjectVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override T Accept<T>(ICryptoApiObjectVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }

    public override void ReComputeAttributes()
    {
        //NOP
    }
}