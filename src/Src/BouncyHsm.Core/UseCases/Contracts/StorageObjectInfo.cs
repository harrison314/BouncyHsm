using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class StorageObjectInfo
{
    public Guid Id
    {
        get;
        set;
    }

    public string CkLabel
    {
        get;
        set;
    }

    public string? CkIdHex
    {
        get;
        set;
    }

    public CKO Type
    {
        get;
        set;
    }

    public CKK? KeyType
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public StorageObjectInfo()
    {
        this.CkLabel = string.Empty;
        this.Description = string.Empty;
    }
}