using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.Slot;

public class CreateTokenDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(512)]
    public string Label
    {
        get;
        set;
    }

    [MinLength(2)]
    [MaxLength(64)]
    public string? SerialNumber
    {
        get;
        set;
    }

    public bool SimulateHwRng
    {
        get;
        set;
    }

    public bool SimulateHwMechanism
    {
        get;
        set;
    }

    public bool SimulateQualifiedArea
    {
        get;
        set;
    }

    [Required]
    [MinLength(1)]
    [MaxLength(120)]
    public string UserPin
    {
        get;
        set;
    }

    [Required]
    [MinLength(1)]
    [MaxLength(120)]
    public string SoPin
    {
        get;
        set;
    }

    [MinLength(1)]
    [MaxLength(120)]
    public string? SignaturePin
    {
        get;
        set;
    }

    public CreateTokenDto()
    {
        this.Label = string.Empty;
        this.UserPin = string.Empty;
        this.SoPin = string.Empty;
    }
}