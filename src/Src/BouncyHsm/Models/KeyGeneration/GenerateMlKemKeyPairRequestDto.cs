using BouncyHsm.Core.Services.Contracts.P11;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration;

public class GenerateMlKemKeyPairRequestDto
{
    [Required]
    public CK_ML_KEM_PARAMETER_SET MlKemParameter
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


    public GenerateMlKemKeyPairRequestDto()
    {
        this.KeyAttributes = default!;
    }
}