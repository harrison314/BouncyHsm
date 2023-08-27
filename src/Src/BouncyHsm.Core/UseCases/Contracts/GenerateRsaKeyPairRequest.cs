namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateRsaKeyPairRequest
{
    public int KeySize
    {
        get;
        set;
    }


    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateRsaKeyPairRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
