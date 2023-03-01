using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Models.StorageObjects;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.StorageObjectInfo))]
public class StorageObjectInfoDto
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

    public StorageObjectInfoDto()
    {
        this.CkLabel = string.Empty;
        this.Description = string.Empty;
    }
}
