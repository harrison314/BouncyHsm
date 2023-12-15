using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Models.Slot;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.Services.Contracts.Entities.TokenInfo))]
public class TokenDto
{
    public string Label
    {
        get;
        set;
    }

    public string SerialNumber
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

    public bool SimulateProtectedAuthPath
    {
        get;
        set;
    }

    public bool IsUserPinLocked
    {
        get;
        set;
    }

    public bool IsSoPinLocked
    {
        get;
        set;
    }

    public SpeedMode SpeedMode
    {
        get;
        set;
    }

    public TokenDto()
    {
        this.SerialNumber = string.Empty;
        this.Label = string.Empty;
    }
}
