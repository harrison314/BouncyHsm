using BouncyHsm.Models.KeyGeneration;
using System.ComponentModel.DataAnnotations;

public class GenerateEdwardsKeyPairRequestDto
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

    public GenerateEdwardsKeyPairRequestDto()
    {
        this.OidOrName = default!;
        this.KeyAttributes = default!;
    }
}