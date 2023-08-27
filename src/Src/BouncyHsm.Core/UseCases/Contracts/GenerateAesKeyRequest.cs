namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateAesKeyRequest
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

    public GenerateAesKeyRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
