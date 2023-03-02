namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.PkcsObjectInfo))]
public class PkcsObjectInfoDto
{
    public string CkaLabel
    {
        get;
        set;
    }

    public byte[] CkaId
    {
        get;
        set;
    }

    public PkcsSpecificObjectDto[] Objects
    {
        get;
        set;
    }

    public bool AlwaysAuthenticate
    {
        get;
        set;
    }

    public string? Subject
    {
        get;
        set;
    }

    public PkcsObjectInfoDto()
    {
        this.CkaLabel = string.Empty;
        this.CkaId = Array.Empty<byte>();
        this.Objects = Array.Empty<PkcsSpecificObjectDto>();
    }
}
