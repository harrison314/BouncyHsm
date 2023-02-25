namespace BouncyHsm.Core.UseCases.Contracts;

public class CreateSlotData
{
    public bool IsHwDevice
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public CreateTokenData Token
    {
        get;
        set;
    }

    public CreateSlotData()
    {
        this.Description = string.Empty;
        this.Token = default!;
    }
}
