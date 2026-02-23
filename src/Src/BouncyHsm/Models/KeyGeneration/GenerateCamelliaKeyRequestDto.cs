using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration;

public class GenerateCamelliaKeyRequestDto
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

    public GenerateCamelliaKeyRequestDto()
    {
        this.KeyAttributes = default!;
    }
}
