﻿using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.InMemory;
internal class MemoryPersistentRepository : IPersistentRepository
{
    private readonly ILogger<MemoryPersistentRepository> logger;

    private List<SlotEntity> slots;
    private ConcurrentDictionary<StorageObjectId, StorageObjectMemento> storageObjects;

    public MemoryPersistentRepository(ILogger<MemoryPersistentRepository> logger)
    {
        this.logger = logger;
        this.storageObjects = new ConcurrentDictionary<StorageObjectId, StorageObjectMemento>();

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
                    SignaturePin = null,
                    SpeedMode = SpeedMode.WithoutRestriction
                }
            }
        };
    }

    public ValueTask<SlotIds> CreateSlot(SlotEntity slot, TokenPins? pins, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to CreateSlot.");

        if (slot == null) throw new ArgumentNullException(nameof(slot));
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

        if (this.slots.Any(t => string.Equals(t.Token.SerialNumber, slot.Token.SerialNumber, StringComparison.OrdinalIgnoreCase)))
        {
            this.logger.LogError("Token serial {TokenSerial} already exists.", slot.Token.SerialNumber);
            throw new BouncyHsmStorageException($"Token serial {slot.Token.SerialNumber} already exists.");
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

    public ValueTask DeleteSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DeleteSlot with slotId {slotId}.", slotId);

        this.slots.RemoveAll(t => t.SlotId == slotId);

        List<StorageObjectId> internalIds = this.storageObjects.Keys.Where(t => t.SlotId == slotId).ToList();
        foreach (StorageObjectId internalId in internalIds)
        {
            this.storageObjects.TryRemove(internalId, out _);
        }

        return new ValueTask();
    }

    public ValueTask<IReadOnlyList<SlotEntity>> GetSlots(GetSlotSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlots.");

        if (specification.WithTokenPresent)
        {
            List<SlotEntity> result = new List<SlotEntity>();
            result.AddRange(this.slots.Where(t => !t.IsUnplugged));
            return new ValueTask<IReadOnlyList<SlotEntity>>(result);
        }

        return new ValueTask<IReadOnlyList<SlotEntity>>(this.slots);
    }

    public ValueTask<bool> ExecuteSlotCommand(uint slotId, IPersistentRepositorySlotCommand command, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ExecuteSlotCommand with slotId {slotId}.", slotId);

        if (command == null) throw new ArgumentNullException(nameof(command));

        SlotEntity? slot = this.slots.SingleOrDefault(t => t.SlotId == slotId);
        if (slot == null)
        {
            this.logger.LogError("Slot with id {slotId} does not exists.", slotId);
            throw new BouncyHsmStorageException($"Slot with id {slotId} does not exists.");
        }

        bool changed = command.UpdateSlot(slot);
        if (changed)
        {
            this.logger.LogInformation("Slot with id {slotId} chaned using command {command}.", slotId, command);
        }

        return new ValueTask<bool>(changed);
    }

    public ValueTask StoreObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to StoreObject with slotId {slotId}.", slotId);

        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        this.storageObjects[new StorageObjectId(storageObject.Id, slotId)] = storageObject.ToMemento();

        return new ValueTask();
    }

    public ValueTask UpdateObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to UpdateObject with slotId {slotId}.", slotId);

        StorageObjectId id = new StorageObjectId(storageObject.Id, slotId);
        if (!this.storageObjects.ContainsKey(id))
        {
            throw new BouncyHsmStorageException($"Object with id {storageObject.Id} in slot {slotId} not found.");
        }

        this.storageObjects[id] = storageObject.ToMemento();

        return new ValueTask();
    }

    public ValueTask<IReadOnlyList<StorageObject>> FindObjects(uint slotId, FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to FindObjects with slotId {slotId}.", slotId);

        List<StorageObject> result = this.storageObjects.Values.Where(t => (specification.IsUserLogged || !t.GetCkaPrivate()) && t.IsMatch(specification.Template))
            .Select(t => StorageObjectFactory.CreateFromMemento(t))
            .ToList();

        return new ValueTask<IReadOnlyList<StorageObject>>(result);
    }

    public ValueTask<bool> ValidatePin(SlotEntity slot, CKU userType, string pin, object? context, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ValidatePin with slotId {slotId}, userType {userType}.", slot.Id, userType);

        if (userType == CKU.CKU_USER)
        {
            return new ValueTask<bool>(((InMemoryTokenInfo)slot.Token).UserPin == pin);
        }

        if (userType == CKU.CKU_SO)
        {
            return new ValueTask<bool>(((InMemoryTokenInfo)slot.Token).SoPin == pin);
        }

        if (userType == CKU.CKU_CONTEXT_SPECIFIC)
        {
            return new ValueTask<bool>(((InMemoryTokenInfo)slot.Token).SignaturePin == pin);
        }

        throw new NotSupportedException($"User type {userType} is not supported.");
    }

    public ValueTask SetPin(SlotEntity slot, CKU userType, string newPin, object? context, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to SetPin with slotId {slotId}, userType {userType}.", slot.Id, userType);
        
        if (userType == CKU.CKU_USER)
        {
            ((InMemoryTokenInfo)slot.Token).UserPin = newPin;
            return new ValueTask();
        }

        if (userType == CKU.CKU_SO)
        {
            ((InMemoryTokenInfo)slot.Token).SoPin = newPin;
            return new ValueTask();
        }

        if (userType == CKU.CKU_CONTEXT_SPECIFIC)
        {
            ((InMemoryTokenInfo)slot.Token).SignaturePin = newPin;
            return new ValueTask();
        }

        throw new NotSupportedException($"User type {userType} is not supported.");
    }

    public ValueTask<StorageObject?> TryLoadObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to TryLoadObject with slotId {slotId}, object id {objectId}.", slotId, id);

        if (this.storageObjects.TryGetValue(new StorageObjectId(id, slotId), out StorageObjectMemento? storageObjectMemento))
        {
            return new ValueTask<StorageObject?>(StorageObjectFactory.CreateFromMemento(storageObjectMemento, false));
        }
        else
        {
            return new ValueTask<StorageObject?>(null as StorageObject);
        }
    }

    public ValueTask DestroyObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DestroyObject with slotId {slotId}, object id {objectId}.", slotId, storageObject.Id);

        if (!this.storageObjects.TryRemove(new StorageObjectId(storageObject.Id, slotId), out _))
        {
            throw new BouncyHsmNotFoundException($"Object with id {storageObject.Id} in slot {slotId} not found.");
        }

        return new ValueTask();
    }

    public ValueTask<PersistentRepositoryStats> GetStats(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetStats");

        int privateKeys = this.storageObjects.Values.Count(t => t.IsPrivateKey());
        int certificates = this.storageObjects.Values.Count(t => t.IsX509Certificate());

        return new ValueTask<PersistentRepositoryStats>(new PersistentRepositoryStats(this.slots.Count,
            this.storageObjects.Count,
            privateKeys,
            certificates));
    }
}
