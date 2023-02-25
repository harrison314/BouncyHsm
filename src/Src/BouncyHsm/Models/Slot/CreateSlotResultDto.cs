namespace BouncyHsm.Models.Slot;

public class CreateSlotResultDto
{
    public Guid Id
    {
        get;
        set;
    }

    public int SlotId
    {
        get;
        set;
    }

    public string TokenSerialNumber
    {
        get;
        set;
    }

    public CreateSlotResultDto()
    {
        this.TokenSerialNumber = string.Empty;
    }
}