using BouncyHsm.Models.KeyGeneration;
using System.ComponentModel.DataAnnotations;

public class GenerateMontgomeryKeyPairRequestDto
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

    public GenerateMontgomeryKeyPairRequestDto()
    {
        this.OidOrName = default!;
        this.KeyAttributes = default!;
    }
}