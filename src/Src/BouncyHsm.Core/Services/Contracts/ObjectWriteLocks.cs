using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.Contracts;

/// <summary>
/// Process-wide write lock for stateful key objects (<see cref="HssPrivateKeyObject"/>, e.g. HSS private keys).
/// </summary>
public static class ObjectWriteLocks
{
    private static readonly Dictionary<Guid, LockEntry> entries = new Dictionary<Guid, LockEntry>();

    public static ValueTask<IDisposable?> Acquire(StorageObject storageObject, CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.Assert(storageObject != null);

        if (storageObject is HssPrivateKeyObject)
        {
            return AcquireNullable(storageObject.Id, cancellationToken);
        }

        return new ValueTask<IDisposable?>((IDisposable?)null);
    }

    internal static async ValueTask<IDisposable> Acquire(Guid objectId, CancellationToken cancellationToken)
    {
        LockEntry entry;
        lock (entries)
        {
            if (!entries.TryGetValue(objectId, out LockEntry? existing))
            {
                existing = new LockEntry();
                entries.Add(objectId, existing);
            }

            existing.RefCount++;
            entry = existing;
        }

        try
        {
            await entry.Semaphore.WaitAsync(cancellationToken);
        }
        catch
        {
            ReleaseReference(objectId, entry);
            throw;
        }

        return new Releaser(objectId, entry);
    }

    /// <summary>
    /// True while the object is locked or has waiters (for tests).
    /// </summary>
    internal static bool IsTracked(Guid objectId)
    {
        lock (entries)
        {
            return entries.ContainsKey(objectId);
        }
    }

    private static async ValueTask<IDisposable?> AcquireNullable(Guid objectId, CancellationToken cancellationToken)
    {
        return await Acquire(objectId, cancellationToken);
    }

    private static void ReleaseReference(Guid objectId, LockEntry entry)
    {
        lock (entries)
        {
            entry.RefCount--;
            if (entry.RefCount == 0)
            {
                entries.Remove(objectId);
                entry.Semaphore.Dispose();
            }
        }
    }

    private sealed class LockEntry
    {
        public SemaphoreSlim Semaphore
        {
            get;
        }

        public int RefCount
        {
            get;
            set;
        }

        public LockEntry()
        {
            this.Semaphore = new SemaphoreSlim(1, 1);
            this.RefCount = 0;
        }
    }

    private sealed class Releaser : IDisposable
    {
        private readonly Guid objectId;
        private LockEntry? entry;

        public Releaser(Guid objectId, LockEntry entry)
        {
            this.objectId = objectId;
            this.entry = entry;
        }

        public void Dispose()
        {
            LockEntry? lockEntry = Interlocked.Exchange(ref this.entry, null);
            if (lockEntry != null)
            {
                lockEntry.Semaphore.Release();
                ReleaseReference(this.objectId, lockEntry);
            }
        }
    }
}
