using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Implementation.SlotCommands;

internal class ResetTokenCommand : IPersistentRepositorySlotCommand
{
    private readonly string newLabel;

    public ResetTokenCommand(string newLabel)
    {
        this.newLabel = newLabel;
    }

    public bool UpdateSlot(SlotEntity slotEntity)
    {
        slotEntity.Token.Label = this.newLabel.Trim();
        slotEntity.Token.MonotonicCounter = 0;
        slotEntity.Token.MonotonicCounterHasReset = true;

        return true;
    }
}
