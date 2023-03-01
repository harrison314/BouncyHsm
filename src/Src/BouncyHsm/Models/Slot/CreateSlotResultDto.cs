namespace BouncyHsm.Models.Slot;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.CreateSlotResult))]
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