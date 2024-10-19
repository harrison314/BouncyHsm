using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.Contracts;

public interface IPersistentRepositorySlotCommand
{
    bool UpdateSlot(SlotEntity slotEntity);
}