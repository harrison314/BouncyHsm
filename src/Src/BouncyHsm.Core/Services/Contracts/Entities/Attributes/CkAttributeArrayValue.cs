using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class CkAttributeArrayValue : IAttributeValue
{
    private readonly IReadOnlyDictionary<CKA, IAttributeValue> value;

    public static CkAttributeArrayValue Empty
    {
        get;
    } = new CkAttributeArrayValue(ReadOnlyDictionary<CKA, IAttributeValue>.Empty);

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.CkAttributeArray;
    }

    public bool IsEmpty
    {
        get => this.value.Count != 0;
    }

    public CkAttributeArrayValue(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.value = template;
    }

    public bool AsBool()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.CkAttributeArray);
    }

    public byte[] AsByteArray()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.CkAttributeArray);
    }

    public CkDate AsDate()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.CkAttributeArray);
    }

    public string AsString()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.CkAttributeArray);
    }

    public uint AsUint()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.CkAttributeArray);
    }

    public uint[] AsUintArray()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.CkAttributeArray);
    }

    public IReadOnlyDictionary<CKA, IAttributeValue> AsCkAttributeArray()
    {
        return this.value;
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other != null && other.TypeTag == AttrTypeTag.CkAttributeArray)
        {
            return BouncyHsm.Core.Services.P11Handlers.Common.AttrTypeUtils.Equals(this.value, other.AsCkAttributeArray());
        }

        return false;
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return AttrTypeUtils.GuessSize(this.value);
    }

    public override string ToString()
    {
        return $"{this.GetType().Name}: Items count {this.value.Count}";
    }
}
