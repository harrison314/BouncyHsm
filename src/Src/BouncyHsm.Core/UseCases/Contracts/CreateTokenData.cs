using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Contracts;

public class CreateTokenData
{
    public string Label
    {
        get;
        set;
    }

    public string? SerialNumber
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

    public string UserPin
    {
        get;
        set;
    }

    public string SoPin
    {
        get;
        set;
    }

    public string? SignaturePin
    {
        get;
        set;
    }

    public SpeedMode SpeedMode
    {
        get;
        set;
    }

    public CreateTokenData()
    {
        this.Label = string.Empty;
        this.UserPin = string.Empty;
        this.SoPin = string.Empty;
    }
}