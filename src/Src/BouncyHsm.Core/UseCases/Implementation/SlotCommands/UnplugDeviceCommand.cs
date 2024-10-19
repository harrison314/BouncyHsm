using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation.SlotCommands;

internal class UnplugDeviceCommand : IPersistentRepositorySlotCommand
{
    public UnplugDeviceCommand()
    {

    }

    public bool UpdateSlot(SlotEntity slotEntity)
    {
        System.Diagnostics.Debug.Assert(slotEntity.IsRemovableDevice);

        if (slotEntity.IsUnplugged)
        {
            return false;
        }
        else
        {
            slotEntity.IsUnplugged = true;
            return true;
        }
    }
}