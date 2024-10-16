
namespace BouncyHsm.Models.HsmInfo;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.MechanismProfile))]
public class MechanismProfileDto
{
    public string? ProfileName 
    {
        get; 
        set;
    }

    public List<MechanismInfoDto> Mechanisms 
    { 
        get; 
        set; 
    }

    public MechanismProfileDto()
    {
        this.Mechanisms = new List<MechanismInfoDto>();
    }
}