namespace BouncyHsm.DocGenerator;

public class ParsedMechanismFlags
{
    public bool Digest
    {
        get;
        set;
    }

    public bool SignAndVerify
    {
        get;
        set;
    }

    public bool SignAndVerifyRecover
    {
        get;
        set;
    }

    public bool Derive
    {
        get;
        set;
    }

    public bool EncryptAndDecrypt
    {
        get;
        set;
    }

    public bool GenerateKeyPair
    {
        get;
        set;
    }

    public bool Generate
    {
        get;
        set;
    }

    public bool WrapAndUnwrap
    {
        get;
        set;
    }

    public ParsedMechanismFlags()
    {
        
    }
}
