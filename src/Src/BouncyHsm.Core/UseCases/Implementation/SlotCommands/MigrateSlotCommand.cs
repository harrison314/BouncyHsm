using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.UseCases.Implementation.SlotCommands;

internal class MigrateSlotCommand : IPersistentRepositorySlotCommand
{
    public MigrateSlotCommand()
    {
        
    }

    public bool UpdateSlot(SlotEntity slotEntity)
    {
        // Migrate from version 1.x
        slotEntity.IsPlugged = true;

        return true;
    }
}
