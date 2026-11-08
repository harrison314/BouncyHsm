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
/// No code path holds two object locks at the same time; keep it that way when adding new callers.
/// </para>
/// </remarks>
public static class ObjectWriteLocks
{
    private const int StripeCount = 256;

    private static readonly SemaphoreSlim[] stripes = CreateStripes();

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
        SemaphoreSlim semaphore = stripes[GetStripeIndex(objectId)];
        await semaphore.WaitAsync(cancellationToken);
        return new Releaser(semaphore);
    }

    internal static int GetStripeIndex(Guid objectId)
    {
        return (int)((uint)objectId.GetHashCode() & (StripeCount - 1));
    }

    private static async ValueTask<IDisposable?> AcquireNullable(Guid objectId, CancellationToken cancellationToken)
    {
        return await Acquire(objectId, cancellationToken);
    }

    private static SemaphoreSlim[] CreateStripes()
    {
        System.Diagnostics.Debug.Assert((StripeCount & (StripeCount - 1)) == 0, "StripeCount must be a power of two.");

        SemaphoreSlim[] result = new SemaphoreSlim[StripeCount];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = new SemaphoreSlim(1, 1);
        }

        return result;
    }

    private sealed class Releaser : IDisposable
    {
        private SemaphoreSlim? semaphore;

        public Releaser(SemaphoreSlim semaphore)
        {
            this.semaphore = semaphore;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref this.semaphore, null)?.Release();
        }
    }
}
