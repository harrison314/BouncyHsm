using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities.Attributes;

internal class TemplateAttributeValue : IAttributeValue
{
    private readonly IReadOnlyDictionary<CKA, IAttributeValue> template;

    public static TemplateAttributeValue Empty
    {
        get;
    } = new TemplateAttributeValue(ReadOnlyDictionary<CKA, IAttributeValue>.Empty);

    public AttrTypeTag TypeTag
    {
        get => AttrTypeTag.Template;
    }

    public TemplateAttributeValue(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.template = template;
    }

    public TemplateAttributeValue(byte[] memntoBytes)
    {
        //TODO: lazy initialization - for memory save - deserilize if need
        StorageObjectMemento memnto = StorageObjectMemento.FromInstance(memntoBytes);
        this.template = memnto.Values;
    }

    public bool AsBool()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.Template);
    }

    public byte[] AsByteArray()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.Template);
    }

    public CkDate AsDate()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.Template);
    }

    public string AsString()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.Template);
    }

    public uint AsUint()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.Template);
    }

    public uint[] AsUintArray()
    {
        throw new InvalidAttributeTypeCastException(AttrTypeTag.Template);
    }

    public IReadOnlyDictionary<CKA, IAttributeValue> AsTemplate()
    {
        return this.template;
    }

    public bool Equals(IAttributeValue? other)
    {
        if (other != null && other.TypeTag == AttrTypeTag.Template)
        {
            return BouncyHsm.Core.Services.P11Handlers.Common.AttrTypeUtils.Equals(this.template, other.AsTemplate());
        }

        return false;
    }

    public bool Equals(uint other)
    {
        return false;
    }

    public uint GuessSize()
    {
        return AttrTypeUtils.GuessSize(this.template);
    }

    public override string ToString()
    {
        return $"{this.GetType().Name}: Items count {this.template.Count}";
    }
}
