namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateMontgomeryKeyPairRequest
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

    public GenerateMontgomeryKeyPairRequest()
    {
        this.OidOrName = string.Empty;
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}