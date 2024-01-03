namespace BouncyHsm.Models.Slot;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.Services.Contracts.Entities.SlotEntity))]
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

    public TokenDto Token
    {
        get;
        set;
    }

    public SlotDto()
    {
        this.Description = string.Empty;
        this.Token = default!;
    }
}
