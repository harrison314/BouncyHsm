using BouncyHsm.Models.KeyGeneration;
using System.ComponentModel.DataAnnotations;

public class GenerateSecretKeyRequestDto
{
    [Required]
    public int Size
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

    public GenerateSecretKeyRequestDto()
    {
        this.KeyAttributes = default!;
    }
}