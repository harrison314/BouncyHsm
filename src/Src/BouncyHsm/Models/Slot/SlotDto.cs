namespace BouncyHsm.Models.Slot;

public class SlotDto
{
    public Guid Id
    {
        get;
        set;
    }

    public uint SlotId
    {
        get;
        set;
    }

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

    public TokenDto? Token
    {
        get;
        set;
    }

    public SlotDto()
    {
        this.Description = string.Empty;
    }
}
