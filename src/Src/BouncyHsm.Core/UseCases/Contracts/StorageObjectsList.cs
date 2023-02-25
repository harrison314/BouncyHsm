namespace BouncyHsm.Core.UseCases.Contracts;

public class StorageObjectsList
{
    public int TotalCount
    {
        get;
        set;
    }

    public List<StorageObjectInfo> Objects
    {
        get;
        set;
    }

    public StorageObjectsList()
    {
        this.Objects = default!;
    }
}
