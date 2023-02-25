
namespace BouncyHsm.Models.HsmInfo;

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
