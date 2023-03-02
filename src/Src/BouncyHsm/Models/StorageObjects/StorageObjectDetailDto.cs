namespace BouncyHsm.Models.StorageObjects;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.StorageObjectDetail))]
public class StorageObjectDetailDto
{
    public Guid Id
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public List<StorageObjectAttributeDto> Attributes
    {
        get;
        set;
    }

    public StorageObjectDetailDto()
    {
        this.Description = string.Empty;
        this.Attributes = default!;
    }
}
