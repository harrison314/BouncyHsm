using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Tests.Services.Contracts;

[TestClass]
public class ObjectWriteLocksTests
{
    [TestMethod]
    public async Task Acquire_SameId_Serializes()
    {
        Guid id = Guid.NewGuid();
        IDisposable first = await ObjectWriteLocks.Acquire(id, CancellationToken.None);

        Task<IDisposable> second = ObjectWriteLocks.Acquire(id, CancellationToken.None).AsTask();
        await Task.Delay(100);
        Assert.IsFalse(second.IsCompleted);

        first.Dispose();
        IDisposable secondLock = await second.WaitAsync(TimeSpan.FromSeconds(10));
        secondLock.Dispose();
    }

    [TestMethod]
    public async Task Acquire_DifferentIds_Independent()
    {
        using IDisposable first = await ObjectWriteLocks.Acquire(Guid.NewGuid(), CancellationToken.None);

        using IDisposable second = await ObjectWriteLocks.Acquire(Guid.NewGuid(), CancellationToken.None)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(10));
    }

    [TestMethod]
    public async Task Release_LastHolder_RemovesEntry()
    {
        Guid id = Guid.NewGuid();
        Assert.IsFalse(ObjectWriteLocks.IsTracked(id));

        IDisposable first = await ObjectWriteLocks.Acquire(id, CancellationToken.None);
        Assert.IsTrue(ObjectWriteLocks.IsTracked(id));

        first.Dispose();
        Assert.IsFalse(ObjectWriteLocks.IsTracked(id));
    }

    [TestMethod]
    public async Task Release_WithWaiter_KeepsEntryUntilWaiterReleases()
    {
        Guid id = Guid.NewGuid();
        IDisposable first = await ObjectWriteLocks.Acquire(id, CancellationToken.None);
        Task<IDisposable> second = ObjectWriteLocks.Acquire(id, CancellationToken.None).AsTask();
        await Task.Delay(50);

        first.Dispose();
        IDisposable secondLock = await second.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsTrue(ObjectWriteLocks.IsTracked(id));

        secondLock.Dispose();
        Assert.IsFalse(ObjectWriteLocks.IsTracked(id));
    }

    [TestMethod]
    public async Task Acquire_CancelledWhileWaiting_RemovesEntryAfterRelease()
    {
        Guid id = Guid.NewGuid();
        IDisposable first = await ObjectWriteLocks.Acquire(id, CancellationToken.None);

        using CancellationTokenSource cts = new CancellationTokenSource();
        Task<IDisposable> waiting = ObjectWriteLocks.Acquire(id, cts.Token).AsTask();
        await Task.Delay(50);
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await waiting);

        first.Dispose();
        Assert.IsFalse(ObjectWriteLocks.IsTracked(id));
    }

    [TestMethod]
    public async Task Acquire_ManyConcurrentWaiters_SerializeAndCleanUp()
    {
        Guid id = Guid.NewGuid();
        int inside = 0;
        int maxInside = 0;

        await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => Task.Run(async () =>
        {
            using IDisposable writeLock = await ObjectWriteLocks.Acquire(id, CancellationToken.None);
            int now = Interlocked.Increment(ref inside);
            InterlockedMax(ref maxInside, now);
            await Task.Yield();
            Interlocked.Decrement(ref inside);
        }))).WaitAsync(TimeSpan.FromSeconds(30));

        Assert.AreEqual(1, maxInside);
        Assert.IsFalse(ObjectWriteLocks.IsTracked(id));
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while ((current = Volatile.Read(ref target)) < value
            && Interlocked.CompareExchange(ref target, value, current) != current)
        {
        }
    }

    [TestMethod]
    public async Task Release_DisposeTwice_ReleasesOnce()
    {
        Guid id = Guid.NewGuid();
        IDisposable first = await ObjectWriteLocks.Acquire(id, CancellationToken.None);
        first.Dispose();
        first.Dispose();

        IDisposable second = await ObjectWriteLocks.Acquire(id, CancellationToken.None);
        Task<IDisposable> third = ObjectWriteLocks.Acquire(id, CancellationToken.None).AsTask();
        await Task.Delay(100);
        Assert.IsFalse(third.IsCompleted);

        second.Dispose();
        IDisposable thirdLock = await third.WaitAsync(TimeSpan.FromSeconds(10));
        thirdLock.Dispose();
    }

    [TestMethod]
    public async Task Acquire_Cancelled_Throws()
    {
        Guid id = Guid.NewGuid();
        using IDisposable first = await ObjectWriteLocks.Acquire(id, CancellationToken.None);
        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await ObjectWriteLocks.Acquire(id, cts.Token));
    }

    [TestMethod]
    public async Task Acquire_NonStatefulObject_ReturnsNullWithoutLocking()
    {
        DataObject dataObject = new DataObject() { Id = Guid.NewGuid() };

        using IDisposable held = await ObjectWriteLocks.Acquire(dataObject.Id, CancellationToken.None);
        IDisposable? writeLock = await ObjectWriteLocks.Acquire(dataObject, CancellationToken.None).AsTask().WaitAsync(TimeSpan.FromSeconds(10));

        Assert.IsNull(writeLock);
    }

    [TestMethod]
    public async Task Acquire_StatefulObject_LocksById()
    {
        HssPrivateKeyObject key = new HssPrivateKeyObject() { Id = Guid.NewGuid() };

        IDisposable? first = await ObjectWriteLocks.Acquire(key, CancellationToken.None);
        Assert.IsNotNull(first);

        Task<IDisposable> second = ObjectWriteLocks.Acquire(key.Id, CancellationToken.None).AsTask();
        await Task.Delay(50);
        Assert.IsFalse(second.IsCompleted, "Lock by object and lock by id must be the same lock.");

        first.Dispose();
        using IDisposable secondLock = await second.WaitAsync(TimeSpan.FromSeconds(10));
    }
}
