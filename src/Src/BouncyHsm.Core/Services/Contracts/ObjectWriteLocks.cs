namespace BouncyHsm.Core.Services.Contracts;

/// <summary>
/// Process-wide per-object write lock. Writers must acquire, then (re)load the object,
/// modify, persist and release.
/// </summary>
/// <remarks>
/// Locks are striped: a fixed array of semaphores is shared by all objects, so memory use is bounded.
/// Two object ids mapped to the same stripe only serialize each other. This can not deadlock because
/// no code path holds an object lock while acquiring another one (each caller - SetAttributeValueHandler,
/// DestroyObjectHandler, HssStatefulSigner, StorageObjectsFacade, MigrationFacade and PkcsFacade -
/// takes a single lock, works on a single object and releases it before taking the next one).
/// Keep it that way when adding new callers.
/// </remarks>
public static class ObjectWriteLocks
{
    private const int StripeCount = 256;

    private static readonly SemaphoreSlim[] stripes = CreateStripes();

    public static async ValueTask<IDisposable> Acquire(Guid objectId, CancellationToken cancellationToken)
    {
        SemaphoreSlim semaphore = stripes[GetStripeIndex(objectId)];
        await semaphore.WaitAsync(cancellationToken);
        return new Releaser(semaphore);
    }

    internal static int GetStripeIndex(Guid objectId)
    {
        return (int)((uint)objectId.GetHashCode() & (StripeCount - 1));
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
