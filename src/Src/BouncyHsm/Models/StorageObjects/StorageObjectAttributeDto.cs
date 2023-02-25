using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Models.StorageObjects;

public class StorageObjectAttributeDto
{
    public string AttributeType
    {
        get;
        set;
    }

    public AttrTypeTag TypeTag
    {
        get;
        set;
    }


    public string ValueHex
    {
        get;
        set;
    }

    public string? ValueText
    {
        get;
        set;
    }

    public int Size
    {
        get;
        set;
    }

    public StorageObjectAttributeDto()
    {
        this.AttributeType = string.Empty;
        this.ValueHex = string.Empty;
    }
}