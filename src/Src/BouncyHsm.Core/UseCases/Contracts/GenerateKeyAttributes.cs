namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateKeyAttributes
{
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

    public bool Exportable
    {
        get;
        set;
    }

    public bool Sensitive
    {
        get;
        set;
    }

    public bool ForSigning
    {
        get;
        set;
    }

    public bool ForEncryption
    {
        get;
        set;
    }

    public bool ForDerivation
    {
        get;
        set;
    }

    public bool ForWrap
    {
        get;
        set;
    }

    public GenerateKeyAttributes()
    {
        this.CkaLabel = string.Empty;
    }
}
