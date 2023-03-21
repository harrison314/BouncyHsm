using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;

public class TokenInfoModel
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

    public TokenInfoModel()
    {
        this.Label = string.Empty;
        this.SerialNumber = string.Empty;
    }
}
