namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateCamelliaKeyRequest
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

    public GenerateCamelliaKeyRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}