namespace BouncyHsm.Models.Pkcs;

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
