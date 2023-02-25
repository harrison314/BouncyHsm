
namespace BouncyHsm.Models.HsmInfo;

public class EcCurveInfoDto
{
    public string Kind
    {
        get;
    }

    public string Name 
    { 
        get; 
    }

    public string Oid 
    { 
        get;
    }

    public EcCurveInfoDto(string kind, string name, string oid)
    {
        this.Kind = kind;
        this.Name = name;
        this.Oid = oid;
    }
}
