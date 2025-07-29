using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Implementation.SlotCommands;

internal class ChangeLabelCommand : IPersistentRepositorySlotCommand
{
    private readonly string newLabel;

    public ChangeLabelCommand(string newLabel)
    {
        this.newLabel = newLabel;
    }

    public bool UpdateSlot(SlotEntity slotEntity)
    {
        slotEntity.Token.Label = this.newLabel.Trim();
        return true;
    }
}
