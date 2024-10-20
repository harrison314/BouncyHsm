using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Models.Slot;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.SetTokenPinData))]
public class SetTokenPinDataDto
{
    [Required]
    public Core.Services.Contracts.P11.CKU UserType 
    { 
        get; 
        set;
    }

    [Required]
    [MinLength(1)]
    [MaxLength(120)]
    public string NewPin 
    { 
        get; 
        set; 
    }

    public SetTokenPinDataDto()
    {
        this.NewPin = string.Empty;
    }
}