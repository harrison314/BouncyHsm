using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class StorageObjectAttribute
{
    public CKA AttributeType
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

    public StorageObjectAttribute()
    {
        this.ValueHex = string.Empty;
    }
}