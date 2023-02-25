
namespace BouncyHsm.Models.HsmInfo;

public class MechanismFlags
{
    public bool Encrypt 
    { 
        get; 
        set; 
    }

    public bool Decrypt 
    { 
        get; 
        set;
    }

    public bool Digest 
    { 
        get;
        set;
    }

    public bool Sign 
    { 
        get;
        set;
    }

    public bool SignRecover 
    { 
        get; 
        set;
    }

    public bool Verify
    { 
        get;
        set; 
    }

    public bool VerifyRecover
    {
        get; 
        set;
    }
    public bool Generate 
    { 
        get;
        set;
    }

    public bool GenerateKeyPair 
    { 
        get; 
        set; 
    }

    public bool Wrap 
    { 
        get;
        set; 
    }

    public bool Unwrap 
    { 
        get; 
        set;
    }

    public bool Derive 
    { 
        get; 
        set; 
    }

    public MechanismFlags()
    {

    }
}