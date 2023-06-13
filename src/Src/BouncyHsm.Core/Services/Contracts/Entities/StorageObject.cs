using BouncyHsm.Core.Services.Contracts.P11;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public abstract class StorageObject : Entity, ICryptoApiObject
{
    protected Dictionary<CKA, IAttributeValue> values;

    public abstract CKO CkaClass
    {
        get;
    }

    public bool CkaToken
    {
        get => this.values[CKA.CKA_TOKEN].AsBool();
        set => this.values[CKA.CKA_TOKEN] = AttributeValue.Create(value);
    }

    public bool CkaPrivate
    {
        get => this.values[CKA.CKA_PRIVATE].AsBool();
        set => this.values[CKA.CKA_PRIVATE] = AttributeValue.Create(value);
    }

    public bool CkaModifiable
    {
        get => this.values[CKA.CKA_MODIFIABLE].AsBool();
        set => this.values[CKA.CKA_MODIFIABLE] = AttributeValue.Create(value);
    }

    public bool CkaCopyable
    {
        get => this.values[CKA.CKA_COPYABLE].AsBool();
        set => this.values[CKA.CKA_COPYABLE] = AttributeValue.Create(value);
    }

    public string CkaLabel
    {
        get => this.values[CKA.CKA_LABEL].AsString();
        set => this.values[CKA.CKA_LABEL] = AttributeValue.Create(value);
    }

    public bool CkaDestroyable
    {
        get => this.values[CKA.CKA_DESTROYABLE].AsBool();
        set => this.values[CKA.CKA_DESTROYABLE] = AttributeValue.Create(value);
    }

    public StorageObject()
    {
        this.values = new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_CLASS, AttributeValue.Create((uint)this.CkaClass) }
        };

        this.CkaToken = false;
        this.CkaPrivate = false;
        this.CkaModifiable = true;
        this.CkaCopyable = true;
        this.CkaDestroyable = true;
        this.CkaLabel = string.Empty;
    }

    internal StorageObject(StorageObjectMemento memento)
    {
        this.Id = memento.Id;
        this.values = new Dictionary<CKA, IAttributeValue>(memento.Values);
    }

    public virtual void SetValue(CKA attributeType, IAttributeValue value, bool isUpdating)
    {
        if (!this.values.ContainsKey(attributeType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_TYPE_INVALID,
                $"Attribute type {attributeType} is not supported in {this.GetType().Name}.");
        }

        if (attributeType == CKA.CKA_CLASS)
        {
            // skip readonly properties
            return;
        }

        if (this.IsReadOnlyAttribute(attributeType))
        {
            throw new RpcPkcs11Exception(CKR.CKR_ATTRIBUTE_READ_ONLY, $"Attribute {attributeType} in object {this.GetType().Name} is read only.");
        }

        if (isUpdating && this.IsSensitiveAttribute(attributeType))
        {
            if (this.values.ContainsKey(CKA.CKA_ALWAYS_SENSITIVE))
            {
                this.values[CKA.CKA_ALWAYS_SENSITIVE] = AttributeValue.Create(false);
            }
        }

        this.values[attributeType] = value;
    }

    public AttributeValueResult GetValue(CKA attributeType)
    {
        if (!this.values.TryGetValue(attributeType, out IAttributeValue? value))
        {
            return new AttributeValueResult.InvalidAttribute();
        }

        if (this.IsSensitiveAttribute(attributeType))
        {
            return new AttributeValueResult.SensitiveOrUnextractable();
        }

        return new AttributeValueResult.Ok(value);
    }

    public virtual void Validate()
    {
        // NOP
    }

    public virtual bool IsMatch(IEnumerable<KeyValuePair<CKA, IAttributeValue>> matchTemplate)
    {
        foreach ((CKA attrType, IAttributeValue attrValue) in matchTemplate)
        {
            if (!this.values.TryGetValue(attrType, out IAttributeValue? value))
            {
                return false;
            }

            if (!value.Equals(attrValue))
            {
                return false;
            }
        }

        return true;
    }

    public uint? TryGetSize(bool isLoggedIn)
    {
        if (this.CkaPrivate)
        {
            return (isLoggedIn) ? this.CalculateObjectSize() : null;
        }
        else
        {
            return this.CalculateObjectSize();
        }
    }

    public StorageObjectMemento ToMemento()
    {
        return new StorageObjectMemento(this.Id, this.values);
    }

    public abstract void Accept(ICryptoApiObjectVisitor visitor);

    public abstract T Accept<T>(ICryptoApiObjectVisitor<T> visitor);

    public abstract void ReComputeAttributes();

    protected virtual bool IsSensitiveAttribute(CKA attributeType)
    {
        return false;
    }

    protected virtual bool IsReadOnlyAttribute(CKA attributeType)
    {
        return attributeType is CKA.CKA_ALWAYS_SENSITIVE
            or CKA.CKA_NEVER_EXTRACTABLE
            or CKA.CKA_ALWAYS_AUTHENTICATE
            or CKA.CKA_LOCAL;
    }

    public override string ToString()
    {
        return $"{this.GetType().Name}: Id={this.Id}";
    }

    private uint CalculateObjectSize()
    {
        uint size = 4 + 6;
        foreach (IAttributeValue attrValue in this.values.Values)
        {
            size += 8;
            size += attrValue.GuessSize();
        }

        return size;
    }
}
