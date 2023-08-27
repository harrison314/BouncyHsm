namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateSecretKeyRequest
{
    public int Size
    {
        get;
        set;
    }

    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateSecretKeyRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}