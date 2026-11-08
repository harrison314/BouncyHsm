using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateHssKeyPairRequest
{
    public uint Levels
    {
        get;
        set;
    }

    public CK_LMS_TYPE[] LmsTypes
    {
        get;
        set;
    }

    public CK_LMOTS_TYPE[] LmotsTypes
    {
        get;
        set;
    }

    public GenerateKeyAttributes KeyAttributes
    {
        get;
        set;
    }

    public GenerateHssKeyPairRequest()
    {
        this.LmsTypes = Array.Empty<CK_LMS_TYPE>();
        this.LmotsTypes = Array.Empty<CK_LMOTS_TYPE>();
        this.KeyAttributes = new GenerateKeyAttributes();
    }
}
