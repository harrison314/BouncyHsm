namespace BouncyHsm.Models.Pkcs;

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

    public PkcsObjectInfoDto()
    {
        this.CkaLabel = string.Empty;
        this.CkaId = Array.Empty<byte>();
        this.Objects = Array.Empty<PkcsSpecificObjectDto>();
    }
}
