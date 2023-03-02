
namespace BouncyHsm.Models.HsmInfo;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.MechanismInfoData))]
public class MechanismInfoDto
{
    public string MechanismType
    {
        get;
        set;
    }

    public int MinKeySize
    {
        get;
        set;
    }

    public int MaxKeySize
    {
        get;
        set;
    }

    public MechanismFlags Flags
    {
        get;
        set;
    }

    public MechanismInfoDto()
    {
        this.MechanismType = string.Empty;
        this.Flags = default!;
    }
}
