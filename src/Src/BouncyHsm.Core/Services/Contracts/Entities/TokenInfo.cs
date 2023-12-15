namespace BouncyHsm.Core.Services.Contracts.Entities;

public class TokenInfo
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

    public TokenInfo()
    {
        this.Label = string.Empty;
        this.SerialNumber = string.Empty;
        this.SpeedMode = SpeedMode.WithoutRestriction;
    }
}
