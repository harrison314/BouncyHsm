namespace BouncyHsm.Core.UseCases.Contracts;

public class StorageObjectDetail
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

    public List<StorageObjectAttribute> Attributes
    {
        get;
        set;
    }

    public StorageObjectDetail()
    {
        this.Description = string.Empty;
        this.Attributes = new List<StorageObjectAttribute>();
    }
}
