using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class HighLevelAttributeValue
{
    public AttrTypeTag TypeTag
    {
        get;
        set;
    }

    public byte[]? ValueAsByteArray
    {
        get;
        set;
    }

    public uint[]? ValueAttributeArray
    {
        get;
        set;
    }

    public bool? ValueAsBool
    {
        get;
        set;
    }

    public uint? ValueAsUint
    {
        get;
        set;
    }

    public string? ValueAsDateTime
    {
        get;
        set;
    }

    public string? ValueAsString
    {
        get;
        set;
    }

    public HighLevelAttributeValue()
    {

    }

    internal HighLevelAttributeValue(IAttributeValue attributeValue)
    {
        System.Diagnostics.Debug.Assert(attributeValue != null);

        this.TypeTag = attributeValue.TypeTag;
        switch (attributeValue.TypeTag)
        {
            case AttrTypeTag.ByteArray:
                this.ValueAsByteArray = attributeValue.AsByteArray();
                break;

            case AttrTypeTag.CkAttributeArray:
                throw new NotSupportedException("CkAttributeArray is not supported in high-level attribute values.");

            case AttrTypeTag.CkBool:
                this.ValueAsBool = attributeValue.AsBool();
                break;

            case AttrTypeTag.CkUint:
                this.ValueAsUint = attributeValue.AsUint();
                break;

            case AttrTypeTag.DateTime:
                this.ValueAsDateTime = attributeValue.AsDate().ToString();
                break;

            case AttrTypeTag.String:
                this.ValueAsString = attributeValue.AsString();
                break;

            default:
                throw new InvalidProgramException($"Enum value {attributeValue.TypeTag} is not supported.");
        }

    }

    internal IAttributeValue ToAttributeValue()
    {
        return this.TypeTag switch
        {
            AttrTypeTag.ByteArray => AttributeValue.Create(this.ValueAsByteArray
                ?? throw new InvalidDataException("Attribute is not byte array.")),
            AttrTypeTag.CkAttributeArray => throw new NotSupportedException("CkAttributeArray is not supported in high-level attribute values."),
            AttrTypeTag.CkBool => AttributeValue.Create(this.ValueAsBool
                ?? throw new InvalidDataException("Attribute is not bool.")),
            AttrTypeTag.CkUint => AttributeValue.Create(this.ValueAsUint
                ?? throw new InvalidDataException("Attribute is not uint.")),
            AttrTypeTag.DateTime => AttributeValue.Create(CkDate.Parse(this.ValueAsDateTime ?? throw new InvalidDataException("Attribute is not DateTime."))),
            AttrTypeTag.String => AttributeValue.Create(this.ValueAsString
                ?? throw new InvalidDataException("Attribute is not string.")),
            _ => throw new InvalidProgramException($"Enum value {this.TypeTag} is not supported."),
        };
    }
}