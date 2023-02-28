namespace BouncyHsm.Models.Pkcs;

public class SubjectNameDto
{
    public string? DirName
    {
        get;
        set;
    }

    public List<SubjectNameEntryDto>? OidValuePairs
    {
        get;
        set;
    }

    public SubjectNameDto()
    {

    }
}
