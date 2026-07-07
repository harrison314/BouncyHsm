namespace BouncyHsm.Models.Migration;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.MigrationResult), IgnoredMembers = new string[] { "SlotId" })]
public class MigrationResultDto
{
    public int FailedObjects
    {
        get;
        set;
    }

    public int SucceededObjects
    {
        get;
        set;
    }

    public MigrationResultDto()
    {

    }
}
