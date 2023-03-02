namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.PkcsObjects))]
public class PkcsObjectsDto
{
    public List<PkcsObjectInfoDto> Objects
    {
        get;
        set;
    }

    public PkcsObjectsDto()
    {
        this.Objects = default!;
    }
}
