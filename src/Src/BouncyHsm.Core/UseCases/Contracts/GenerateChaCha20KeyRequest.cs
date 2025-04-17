namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateChaCha20KeyRequest
{
    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateChaCha20KeyRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}