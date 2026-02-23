using BouncyHsm.Core.Services.Contracts.P11;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration;

public class GenerateSlhDsaKeyPairRequestDto
{
    [Required]
    public CK_SLH_DSA_PARAMETER_SET SlhDsaParameter
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


    public GenerateSlhDsaKeyPairRequestDto()
    {
        this.KeyAttributes = default!;
    }
}