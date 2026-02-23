using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateSlhDsaKeyPairRequest
{
    public CK_SLH_DSA_PARAMETER_SET SlhDsaParameter
    {
        get;
        set;
    }

    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateSlhDsaKeyPairRequest()
    {
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}