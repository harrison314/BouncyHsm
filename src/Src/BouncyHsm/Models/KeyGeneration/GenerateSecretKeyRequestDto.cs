using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration;

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