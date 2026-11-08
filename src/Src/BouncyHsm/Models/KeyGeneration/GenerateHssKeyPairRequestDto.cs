using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.KeyGeneration;

public class GenerateHssKeyPairRequestDto
{
    [Required]
    public uint Levels
    {
        get;
        set;
    }

    /// <summary>LMS type names, one per level (CK_LMS_TYPE).</summary>
    [Required]
    public string[] LmsTypes
    {
        get;
        set;
    }

    /// <summary>LM-OTS type names, one per level (CK_LMOTS_TYPE).</summary>
    [Required]
    public string[] LmotsTypes
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

    public GenerateHssKeyPairRequestDto()
    {
        this.LmsTypes = default!;
        this.LmotsTypes = default!;
        this.KeyAttributes = default!;
    }
}
