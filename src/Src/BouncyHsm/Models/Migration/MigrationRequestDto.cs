namespace BouncyHsm.Models.Migration;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.MigrationRequest))]
public class MigrationRequestDto
{
    public bool ResetAllowedMechanism
    {
        get;
        set;
    }

    public MigrationRequestDto()
    {

    }
}
