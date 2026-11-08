using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.Contracts;

/// <summary>
/// Process-wide write lock for stateful key objects (<see cref="IStatefulKeyObject"/>, e.g. HSS private keys).
/// </summary>
/// <remarks>
/// Only stateful objects are locked: for them, persisting a stale copy would rewind the key state
/// and reuse a one-time key. Other objects are written without a lock, as before.
/// <para>
/// Usage: load the object, call <see cref="Acquire(StorageObject, CancellationToken)"/>, and when
/// a lock is returned, reload the object inside it before modifying, persisting or destroying it.
/// The class of an object never changes, so deciding on the first (unlocked) load is safe.
/// </para>
/// <para>
/// A semaphore exists only while its object is locked or waited for: it is created by the first
/// <c>Acquire</c> and removed when the last holder or waiter is done. When no stateful key is in use,
/// nothing is allocated.
/// </para>
/// <para>
/// No code path holds two object locks at the same time; keep it that way when adding new callers.
/// </para>
/// </remarks>
public static class ObjectWriteLocks
{
    private static readonly Dictionary<Guid, LockEntry> entries = new Dictionary<Guid, LockEntry>();

    /// <summary>
    /// Acquires the write lock for <paramref name="storageObject"/> when it is a stateful key object.
    /// </summary>
    /// <returns>The lock to dispose, or <c>null</c> when the object does not need one.</returns>
    public static ValueTask<IDisposable?> Acquire(StorageObject storageObject, CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.Assert(storageObject != null);

        if (storageObject is not IStatefulKeyObject)
        {
            return new ValueTask<IDisposable?>((IDisposable?)null);
        }

        return AcquireNullable(storageObject.Id, cancellationToken);
    }

    /// <summary>
    /// Acquires the write lock by object id. For callers that already know the object is stateful
    /// (e.g. the HSS signer, which must lock before it loads the key).
    /// </summary>
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
        } = new SemaphoreSlim(1, 1);

        // Holder plus waiters; guarded by the entries dictionary lock.
        public int RefCount
        {
            get;
            set;
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
