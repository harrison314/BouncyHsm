namespace BouncyHsm.Core.UseCases.Contracts;

public class ImportPemRequest
{
    public uint SlotId
    {
        get;
        set;
    }

    public string Pem
    {
        get;
        set;
    }

    public string? Password
    {
        get;
        set;
    }

    public string CkaLabel
    {
        get;
        set;
    }

    public byte[]? CkaId
    {
        get;
        set;
    }

    public ImportPemHints Hints
    {
        get;
        set;
    }

    public ImportPemRequest()
    {
        this.Pem = string.Empty;
        this.CkaLabel = string.Empty;
        this.Hints = new ImportPemHints();
    }

}
