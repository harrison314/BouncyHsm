namespace BouncyHsm.Infrastructure.Storage.LiteDbFile;

public class LiteDbPersistentRepositorySetup
{
    public string DbFilePath
    {
        get;
        set;
    }

    public bool ReadOnly
    {
        get;
        set;
    }

    public bool ReduceLogFileSize
    {
        get;
        set;
    }

    public LiteDbPersistentRepositorySetup()
    {
        this.DbFilePath = string.Empty;
    }
}
