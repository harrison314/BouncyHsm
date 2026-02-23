using BouncyHsm.Core.Services.Contracts.P11;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration;

public class GenerateMLDsaKeyPairRequestDto
{
    [Required]
    public CK_ML_DSA_PARAMETER_SET MlDsaParameter
    {
        get;
        set;
    }

    [Required]
    public GenerateKeyAttributesDto KeyAttributes
    {
        get;
        set;
    }


    public GenerateMLDsaKeyPairRequestDto()
    {
        this.KeyAttributes = default!;
    }
}
