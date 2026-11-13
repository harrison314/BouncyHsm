using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Infrastructure.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.InMemory;

internal class MemoryPersistentRepository : IPersistentRepository, IDisposable
{
    private readonly ILogger<MemoryPersistentRepository> logger;
    private readonly List<SlotEntity> slots;
    private readonly ConcurrentDictionary<StorageObjectId, StorageObjectMemento> storageObjects;
    private readonly ReaderWriterLockSlim readerWriterLock;
    private bool disposedValue;
    private uint slotCounter;

    public MemoryPersistentRepository(ILogger<MemoryPersistentRepository> logger)
    {
        this.logger = logger;
        this.storageObjects = new ConcurrentDictionary<StorageObjectId, StorageObjectMemento>();
        this.readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        this.disposedValue = false;
        this.slotCounter = 100;

        this.slots = new List<SlotEntity>()
        {
            new SlotEntity()
            {
                Id = Guid.NewGuid(),
                SlotId = this.GetNewSlotId(),
                Description = "Example token with PIN 123456",
                IsHwDevice = false,
                IsPlugged = true,
                IsRemovableDevice = false,
                Token = new InMemoryTokenInfo()
                {
                    Label = "Example token",
                    SerialNumber = "000000000001",
                    IsSoPinLocked = false,
                    IsUserPinLocked= false,
                    SimulateHwRng = true,
                    SimulateHwMechanism = true,
                    SimulateQualifiedArea = false,
                    MonotonicCounter = 0,
                    MonotonicCounterHasReset = false,
                    SimulateProtectedAuthPath = false,

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

        this.readerWriterLock.EnterWriteLock();
        try
        {
            SlotEntity slotEntity = new SlotEntity()
            {
                Id = Guid.NewGuid(),
                SlotId = this.GetNewSlotId(),
                Description = slot.Description,
                IsHwDevice = slot.IsHwDevice,
                IsRemovableDevice = slot.IsRemovableDevice,
                IsPlugged = slot.IsPlugged,
                Token = new InMemoryTokenInfo()
                {
                    Label = slot.Token.Label,
                    SerialNumber = slot.Token.SerialNumber,
                    IsSoPinLocked = slot.Token.IsSoPinLocked,
                    IsUserPinLocked = slot.Token.IsUserPinLocked,
                    SimulateHwRng = slot.Token.SimulateHwRng,
                    SimulateHwMechanism = slot.Token.SimulateHwMechanism,
                    SimulateQualifiedArea = slot.Token.SimulateQualifiedArea,
                    MonotonicCounter = 0,
                    MonotonicCounterHasReset = false,
                    SimulateProtectedAuthPath = slot.Token.SimulateProtectedAuthPath,
                    SpeedMode = slot.Token.SpeedMode,

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
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }
    }

    public ValueTask<SlotEntity?> GetSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlot.");

        this.readerWriterLock.EnterReadLock();
        try
        {
            SlotEntity? slot = this.slots.SingleOrDefault(t => t.SlotId == slotId);
            return new ValueTask<SlotEntity?>(slot);
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
    }

    public ValueTask DeleteSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DeleteSlot with slotId {slotId}.", slotId);

        this.readerWriterLock.EnterWriteLock();
        try
        {
            int deletedCount = this.slots.RemoveAll(t => t.SlotId == slotId);

            List<StorageObjectId> internalIds = this.storageObjects.Keys.Where(t => t.SlotId == slotId).ToList();
            foreach (StorageObjectId internalId in internalIds)
            {
                this.storageObjects.TryRemove(internalId, out _);
            }

            if (deletedCount == 0)
            {
                throw new BouncyHsmNotFoundException($"Slot with slotId {slotId} not found.");
            }

            return new ValueTask();
        }
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }
    }

    public ValueTask<IReadOnlyList<SlotEntity>> GetSlots(GetSlotSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlots.");

        this.readerWriterLock.EnterReadLock();
        try
        {
            if (specification.WithTokenPresent)
            {
                List<SlotEntity> result = new List<SlotEntity>();
                result.AddRange(this.slots.Where(t => !t.IsRemovableDevice || (t.IsRemovableDevice && t.IsPlugged)));
                return new ValueTask<IReadOnlyList<SlotEntity>>(result);
            }

            return new ValueTask<IReadOnlyList<SlotEntity>>(this.slots);
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
    }

    public ValueTask<bool> ExecuteSlotCommand(uint slotId, IPersistentRepositorySlotCommand command, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ExecuteSlotCommand with slotId {slotId}.", slotId);

        if (command == null) throw new ArgumentNullException(nameof(command));

        this.readerWriterLock.EnterWriteLock();
        try
        {
            SlotEntity? slot = this.slots.SingleOrDefault(t => t.SlotId == slotId);
            if (slot == null)
            {
                this.logger.LogError("Slot with id {slotId} does not exists.", slotId);
                throw new BouncyHsmNotFoundException($"Slot with id {slotId} does not exists.");
            }

            bool changed = command.UpdateSlot(slot);
            if (changed)
            {
                this.logger.LogInformation("Slot with id {slotId} chaned using command {command}.", slotId, command);
            }

            return new ValueTask<bool>(changed);
        }
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }
    }

    public ValueTask StoreObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to StoreObject with slotId {slotId}.", slotId);

        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        this.readerWriterLock.EnterWriteLock();
        try
        {
            this.storageObjects[new StorageObjectId(storageObject.Id, slotId)] = storageObject.ToMemento();
        }
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }

        return new ValueTask();
    }

    public ValueTask UpdateObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to UpdateObject with slotId {slotId}.", slotId);

        StorageObjectId id = new StorageObjectId(storageObject.Id, slotId);

        this.readerWriterLock.EnterWriteLock();
        try
        {
            if (!this.storageObjects.ContainsKey(id))
            {
                throw new BouncyHsmNotFoundException($"Object with id {storageObject.Id} in slot {slotId} not found.");
            }

            this.storageObjects[id] = storageObject.ToMemento();
        }
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }

        return new ValueTask();
    }

    public ValueTask<IReadOnlyList<StorageObject>> FindObjects(uint slotId, FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to FindObjects with slotId {slotId}.", slotId);

        this.readerWriterLock.EnterReadLock();
        try
        {
            List<StorageObject> result = this.storageObjects
            .Where(t => t.Key.SlotId == slotId)
            .Select(t => t.Value)
            .Where(t => (specification.IsUserLogged || !t.GetCkaPrivate()) && t.IsMatch(specification.Template))
            .Select(t => StorageObjectFactory.CreateFromMemento(t))
            .ToList();

            return new ValueTask<IReadOnlyList<StorageObject>>(result);
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
    }

    public ValueTask<bool> ValidatePin(SlotEntity slot, CKU userType, string pin, object? context, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ValidatePin with slotId {slotId}, userType {userType}.", slot.Id, userType);

        this.readerWriterLock.EnterReadLock();
        try
        {
            if (userType == CKU.CKU_USER)
            {
                return new ValueTask<bool>(StringOperations.FixedTimeEquals(((InMemoryTokenInfo)slot.Token).UserPin, pin));
            }

            if (userType == CKU.CKU_SO)
            {
                return new ValueTask<bool>(StringOperations.FixedTimeEquals(((InMemoryTokenInfo)slot.Token).SoPin, pin));
            }

            if (userType == CKU.CKU_CONTEXT_SPECIFIC)
            {
                if (((InMemoryTokenInfo)slot.Token).SignaturePin == null)
                {
                    return new ValueTask<bool>(false);
                }

                return new ValueTask<bool>(StringOperations.FixedTimeEquals(((InMemoryTokenInfo)slot.Token).SignaturePin, pin));
            }
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }

        throw new InvalidProgramException($"Enum value {userType} is not supported.");
    }

    public ValueTask SetPin(SlotEntity slot, CKU userType, string newPin, object? context, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to SetPin with slotId {slotId}, userType {userType}.", slot.Id, userType);

        this.readerWriterLock.EnterWriteLock();
        try
        {
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
        }
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }

        throw new InvalidProgramException($"Enum value {userType} is not supported.");
    }

    public ValueTask<StorageObject?> TryLoadObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to TryLoadObject with slotId {slotId}, object id {objectId}.", slotId, id);

        this.readerWriterLock.EnterReadLock();
        try
        {
            if (this.storageObjects.TryGetValue(new StorageObjectId(id, slotId), out StorageObjectMemento? storageObjectMemento))
            {
                return new ValueTask<StorageObject?>(StorageObjectFactory.CreateFromMemento(storageObjectMemento));
            }
            else
            {
                return new ValueTask<StorageObject?>(null as StorageObject);
            }
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
    }

    public ValueTask DestroyObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DestroyObject with slotId {slotId}, object id {objectId}.", slotId, storageObject.Id);

        this.readerWriterLock.EnterWriteLock();
        try
        {
            if (!this.storageObjects.TryRemove(new StorageObjectId(storageObject.Id, slotId), out _))
            {
                throw new BouncyHsmNotFoundException($"Object with id {storageObject.Id} in slot {slotId} not found.");
            }
        }
        finally
        {
            this.readerWriterLock.ExitWriteLock();
        }

        return new ValueTask();
    }

    public ValueTask<PersistentRepositoryStats> GetStats(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetStats");

        int privateKeys = this.storageObjects.Values.Count(t => t.IsPrivateKey());
        int certificates = this.storageObjects.Values.Count(t => t.IsX509Certificate());

        this.readerWriterLock.EnterReadLock();
        try
        {
            return new ValueTask<PersistentRepositoryStats>(new PersistentRepositoryStats(this.slots.Count,
              this.storageObjects.Count,
              privateKeys,
              certificates));
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
    }

    public ValueTask CheckHealth(CancellationToken cancellationToken)
    {
        if (this.disposedValue)
        {
            throw new ObjectDisposedException(nameof(MemoryPersistentRepository));
        }

        return new ValueTask();
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.readerWriterLock.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private uint GetNewSlotId()
    {
        unchecked
        {
            return ++this.slotCounter;
        }
    }
}
