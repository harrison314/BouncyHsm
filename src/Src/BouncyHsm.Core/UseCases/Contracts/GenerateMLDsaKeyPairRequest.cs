using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateMLDsaKeyPairRequest
{
    public CK_ML_DSA_PARAMETER_SET MlDsaParameter
    {
        get;
        set;
    }

    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateMLDsaKeyPairRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
