namespace BouncyHsm.Core.UseCases.Contracts;

public class ImportPemHints
{
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

    public PrivateKeyImportMode ImportMode
    {
        get;
        set;
    }

    public ImportPemHints()
    {
        
    }
}