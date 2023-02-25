using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts;

public interface IPersistentRepository
{
    ValueTask<IReadOnlyList<SlotEntity>> GetSlots(GetSlotSpecification specification, CancellationToken cancellationToken);

    ValueTask<SlotEntity?> GetSlot(uint slotId, CancellationToken cancellationToken);

    ValueTask<SlotIds> CreateSlot(SlotEntity slot, TokenPins? pins, CancellationToken cancellationToken);

    ValueTask<bool> ValidatePin(SlotEntity slot, CKU userType, string pin, object? context, CancellationToken cancellationToken);
   
    ValueTask StoreObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken);
   
    ValueTask<IReadOnlyList<StorageObject>> FindObjects(uint slotId, FindObjectSpecification specification, CancellationToken cancellationToken);
   
    ValueTask<StorageObject?> TryLoadObject(uint slotId, Guid id, CancellationToken cancellationToken);

    ValueTask DestroyObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken);
}
