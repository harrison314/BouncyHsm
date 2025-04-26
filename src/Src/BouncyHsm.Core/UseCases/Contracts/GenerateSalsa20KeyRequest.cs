namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateSalsa20KeyRequest
{
    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateSalsa20KeyRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}