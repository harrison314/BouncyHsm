namespace BouncyHsm.Core.UseCases.Contracts;

public class PkcsObjectInfo
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

    public PkcsSpecificObject[] Objects
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

    public PkcsObjectInfo()
    {
        this.CkaLabel = string.Empty;
        this.CkaId = Array.Empty<byte>();
        this.Objects = Array.Empty<PkcsSpecificObject>();
    }
}