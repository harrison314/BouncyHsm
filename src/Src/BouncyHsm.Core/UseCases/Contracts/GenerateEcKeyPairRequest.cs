namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateEcKeyPairRequest
{
    public string OidOrName
    {
        get;
        set;
    }

    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateEcKeyPairRequest()
    {
        this.OidOrName = string.Empty;
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
