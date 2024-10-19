using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.Cache;

public class PersistentRepositoryCache : IPersistentRepository
{
    private readonly IMemoryCache memoryCache;
    private readonly IPersistentRepository repository;

    private const string SlotListKey = "PersistentRepositoryCache_SlotList";
    private const string SlotKeyPrefix = "PersistentRepositoryCache_Slot:";

    public PersistentRepositoryCache(IMemoryCache memoryCache, IPersistentRepository repository)
    {
        this.memoryCache = memoryCache;
        this.repository = repository;
    }

    public async ValueTask<SlotIds> CreateSlot(SlotEntity slot, TokenPins? pins, CancellationToken cancellationToken)
    {
        SlotIds slotid = await this.repository.CreateSlot(slot, pins, cancellationToken);
        this.memoryCache.Remove(SlotListKey);

        return slotid;
    }

    public async ValueTask DeleteSlot(uint slotId, CancellationToken cancellationToken)
    {
        await this.repository.DeleteSlot(slotId, cancellationToken);
        this.memoryCache.Remove(SlotListKey);
        this.memoryCache.Remove($"{SlotKeyPrefix}{slotId}");
    }
    public async ValueTask<SlotEntity?> GetSlot(uint slotId, CancellationToken cancellationToken)
    {
        return await this.memoryCache.GetOrCreateAsync(SlotListKey,
            async (cacheItem) =>
            {
                SlotEntity? item = await this.repository.GetSlot(slotId, cancellationToken);
                cacheItem.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60.0);
                return item;
            });
    }

    public async ValueTask<IReadOnlyList<SlotEntity>> GetSlots(GetSlotSpecification specification, CancellationToken cancellationToken)
    {
        IReadOnlyList<SlotEntity>? slots = await this.memoryCache.GetOrCreateAsync(SlotListKey,
            async (cacheItem) =>
            {
                IReadOnlyList<SlotEntity> items = await this.repository.GetSlots(specification, cancellationToken);
                cacheItem.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60.0);
                return items;
            });

        if (slots == null)
        {
            throw new InvalidProgramException("slot is null response");
        }

        return slots;
    }

    public async ValueTask<bool> ExecuteSlotCommand(uint slotId, IPersistentRepositorySlotCommand command, CancellationToken cancellationToken)
    {
        bool slotChanged = await this.repository.ExecuteSlotCommand(slotId, command, cancellationToken);
        if (slotChanged)
        {
            this.memoryCache.Remove(SlotListKey);
            this.memoryCache.Remove($"{SlotKeyPrefix}{slotId}");
        }

        return slotChanged;
    }

    public ValueTask DestroyObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        return this.repository.DestroyObject(slotId, storageObject, cancellationToken);
    }

    public ValueTask<IReadOnlyList<StorageObject>> FindObjects(uint slotId, FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        return this.repository.FindObjects(slotId, specification, cancellationToken);
    }

    public ValueTask<PersistentRepositoryStats> GetStats(CancellationToken cancellationToken)
    {
        return this.repository.GetStats(cancellationToken);
    }

    public ValueTask StoreObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        return this.repository.StoreObject(slotId, storageObject, cancellationToken);
    }

    public ValueTask<StorageObject?> TryLoadObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        return this.repository.TryLoadObject(slotId, id, cancellationToken);
    }

    public ValueTask UpdateObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        return this.repository.UpdateObject(slotId, storageObject, cancellationToken);
    }

    public ValueTask<bool> ValidatePin(SlotEntity slot, CKU userType, string pin, object? context, CancellationToken cancellationToken)
    {
        return this.repository.ValidatePin(slot, userType, pin, context, cancellationToken);
    }
}
