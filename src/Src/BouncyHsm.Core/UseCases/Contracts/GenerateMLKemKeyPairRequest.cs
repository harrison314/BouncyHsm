using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateMLKemKeyPairRequest
{
    public CK_ML_KEM_PARAMETER_SET MlKemParameter
    {
        get;
        set;
    }

    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateMLKemKeyPairRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
