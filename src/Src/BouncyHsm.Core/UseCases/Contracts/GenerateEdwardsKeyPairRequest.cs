namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateEdwardsKeyPairRequest
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

    public GenerateEdwardsKeyPairRequest()
    {
        this.OidOrName = string.Empty;
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}