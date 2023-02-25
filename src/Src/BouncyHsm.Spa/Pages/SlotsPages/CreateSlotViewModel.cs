using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Pages.SlotsPages;

public class CreateSlotViewModel : IValidatableObject
{
    [Required]
    [MinLength(1)]
    public string Description
    {
        get;
        set;
    }

    public bool SimulateHwSlot
    {
        get;
        set;
    }

    [Required]
    [MinLength(1)]
    public string TokenLabel
    {
        get;
        set;
    }

    public string TokenSerialNumber
    {
        get;
        set;
    }

    [Required]
    [MinLength(4)]
    public string TokenUserPin
    {
        get;
        set;
    }

    [Required]
    [MinLength(4)]
    public string TokenSoPin
    {
        get;
        set;
    }

    public string TokenSignaturePin
    {
        get;
        set;
    }

    public bool TokenSimulateHwMechanism
    {
        get;
        set;
    }

    public bool TokenSimulateHwRng
    {
        get;
        set;
    }

    public bool TokenSimulateQualifiedArea
    {
        get;
        set;
    }

    public CreateSlotViewModel()
    {
        this.Description = string.Empty;
        this.TokenLabel = string.Empty;
        this.TokenSerialNumber = string.Empty;
        this.TokenUserPin = string.Empty;
        this.TokenSoPin = string.Empty;
        this.TokenSignaturePin = string.Empty;

        this.SimulateHwSlot = true;
        this.TokenSimulateHwMechanism = true;
        this.TokenSimulateHwRng = true;
        this.TokenSimulateQualifiedArea = false;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.TokenSimulateQualifiedArea)
        {
            if (string.IsNullOrEmpty(this.TokenSignaturePin))
            {
                yield return new ValidationResult("TokenSignaturePin is required if is TokenSimulateQualifiedArea is set.", new string[] { nameof(this.TokenSignaturePin) });
            }
            else
            {
                if (this.TokenSignaturePin.Length < 4)
                {
                    yield return new ValidationResult("Min length of TokenSignaturePin is 4.", new string[] { nameof(this.TokenSignaturePin) });
                }
            }
        }

        if (!string.IsNullOrEmpty(this.TokenSerialNumber))
        {
            if (this.TokenSerialNumber.Length < 4)
            {
                yield return new ValidationResult("Min length of TokenSerialNumber is 4.", new string[] { nameof(this.TokenSerialNumber) });
            }

            if (this.TokenSerialNumber.Length > 64)
            {
                yield return new ValidationResult("Max length of TokenSerialNumber is 64.", new string[] { nameof(this.TokenSerialNumber) });
            }
        }
    }
}
