using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.InMemory;

internal class MemoryPersistentRepository : IPersistentRepository
{
    private readonly ILogger<MemoryPersistentRepository> logger;

    private List<SlotEntity> slots;
    private ConcurrentDictionary<Guid, StorageObject> storageObjects;

    public MemoryPersistentRepository(ILogger<MemoryPersistentRepository> logger)
    {
        this.logger = logger;
        this.storageObjects = new ConcurrentDictionary<Guid, StorageObject>();

        this.slots = new List<SlotEntity>()
        {
            new SlotEntity()
            {
                Id = Guid.NewGuid(),
                SlotId = 1457,
                Description = "Example",
                IsHwDevice = false,
                Token = new InMemoryTokenInfo()
                {
                    Label = "Example token",
                    SerialNumber = "000000000001",
                    IsSoPinLocked = false,
                    IsUserPinLocked= false,
                    SimulateHwRng = true,
                    SimulateHwMechanism = true,
                    SimulateQualifiedArea = false,

                    UserPin = "123456",
                    SoPin = "12345678",
                    SignaturePin = null
                }
            }
        };
    }

    public ValueTask<SlotIds> CreateSlot(SlotEntity slot, TokenPins? pins, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to CreateSlot.");

        if (slot.Token == null) throw new ArgumentNullException("Token is null");
        if (pins == null) throw new ArgumentNullException(nameof(pins));

        SlotEntity slotEntity = new SlotEntity()
        {
            Id = Guid.NewGuid(),
            SlotId = this.slots.Select(t => t.SlotId).Max() + 1,
            Description = slot.Description,
            IsHwDevice = slot.IsHwDevice,
            Token = new InMemoryTokenInfo()
            {
                Label = slot.Token.Label,
                SerialNumber = slot.Token.SerialNumber,
                IsSoPinLocked = slot.Token.IsSoPinLocked,
                IsUserPinLocked = slot.Token.IsUserPinLocked,
                SimulateHwRng = slot.Token.SimulateHwRng,
                SimulateHwMechanism = slot.Token.SimulateHwMechanism,
                SimulateQualifiedArea = slot.Token.SimulateQualifiedArea,

                UserPin = pins.UserPin,
                SoPin = pins.SoPin,
                SignaturePin = pins.SignaturePin
            }
        };

        if (this.slots.Any(t => string.Equals(t.Token?.SerialNumber, slot.Token.SerialNumber, StringComparison.OrdinalIgnoreCase)))
        {
            this.logger.LogError("Token serial {TokenSerial} alerady exists.", slot.Token.SerialNumber);
            throw new BouncyHsmStorageException($"Token serial {slot.Token.SerialNumber} alerady exists.");
        }

        this.slots.Add(slotEntity);

        this.logger.LogDebug("Create a new slot.");
        return new ValueTask<SlotIds>(new SlotIds(slotEntity.Id, slotEntity.SlotId));
    }

    public ValueTask<SlotEntity?> GetSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlot.");

        SlotEntity? slot = this.slots.SingleOrDefault(t => t.SlotId == slotId);

        return new ValueTask<SlotEntity?>(slot);
    }

    public ValueTask<IReadOnlyList<SlotEntity>> GetSlots(GetSlotSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlots.");

        if (specification.WithTokenPresent)
        {
            List<SlotEntity> result = new List<SlotEntity>();
            result.AddRange(this.slots.Where(t => t.Token != null));
            return new ValueTask<IReadOnlyList<SlotEntity>>(result);
        }

        return new ValueTask<IReadOnlyList<SlotEntity>>(slots);
    }

    public ValueTask StoreObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        this.storageObjects[storageObject.Id] = storageObject;

        return new ValueTask();
    }

    public ValueTask<IReadOnlyList<StorageObject>> FindObjects(uint slotId, FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        List<StorageObject> result = this.storageObjects.Values.Where(t => (specification.IsUserLogged || !t.CkaPrivate) && t.IsMatch(specification.Template)).ToList();

        return new ValueTask<IReadOnlyList<StorageObject>>(result);
    }

    public ValueTask<bool> ValidatePin(SlotEntity slot, CKU userType, string pin, object? context, CancellationToken cancellationToken)
    {
        if (userType == CKU.CKU_USER)
        {
            return new ValueTask<bool>(((InMemoryTokenInfo)slot.Token!).UserPin == pin);
        }

        if (userType == CKU.CKU_SO)
        {
            return new ValueTask<bool>(((InMemoryTokenInfo)slot.Token!).SoPin == pin);
        }

        throw new NotSupportedException();
    }

    public ValueTask<StorageObject?> TryLoadObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        StorageObject? storageObject = null;
        this.storageObjects.TryGetValue(id, out storageObject);

        return new ValueTask<StorageObject?>(storageObject);
    }

    public ValueTask DestroyObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.storageObjects.TryRemove(storageObject.Id, out _);

        return new ValueTask();
    }
}
