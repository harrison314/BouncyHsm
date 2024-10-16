namespace BouncyHsm.Models.Pkcs;

public class ImportPemResponseDto
{
    public IReadOnlyList<Guid> ObjectIds
    {
        get;
        set;
    }

    public ImportPemResponseDto()
    {
        this.ObjectIds = new List<Guid>();
    }

    internal ImportPemResponseDto(IReadOnlyList<Guid> objectIds)
    {
        this.ObjectIds = objectIds;
    }
}