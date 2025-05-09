
namespace BouncyHsm.Models.HsmInfo;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.Services.Contracts.Entities.SupportedNameCurve))]
public class CurveInfoDto
{
    public string Kind
    {
        get;
    }

    public string Name 
    { 
        get; 
    }

    public string? NamedCurve
    {
        get;
    }

    public string Oid 
    { 
        get;
    }

    public CurveInfoDto(string kind, string name,  string oid, string? namedCurve)
    {
        this.Kind = kind;
        this.Name = name;
        this.Oid = oid;
        this.NamedCurve = namedCurve;
    }
}
