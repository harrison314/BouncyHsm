namespace BouncyHsm.Core.UseCases.Contracts;

public class GeneratePoly1305KeyRequest
{
    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GeneratePoly1305KeyRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
