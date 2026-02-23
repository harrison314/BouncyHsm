using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Implementation.SlotCommands;

internal class PlugDeviceCommand : IPersistentRepositorySlotCommand
{
    public PlugDeviceCommand()
    {

    }

    public bool UpdateSlot(SlotEntity slotEntity)
    {
        System.Diagnostics.Debug.Assert(slotEntity.IsRemovableDevice);

        if (slotEntity.IsPlugged)
        {
            return false;
            
        }
        else
        {
            slotEntity.IsPlugged = true;
            return true;
        }
    }
}
