using BouncyHsm.Models.KeyGeneration;
using System.ComponentModel.DataAnnotations;

public class GenerateEcKeyPairRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(256)]
    public string OidOrName
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

    public GenerateEcKeyPairRequestDto()
    {
        this.OidOrName = default!;
        this.KeyAttributes = default!;
    }
}
