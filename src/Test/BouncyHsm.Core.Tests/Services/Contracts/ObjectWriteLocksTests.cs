using BouncyHsm.Core.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
    public async Task Acquire_DifferentStripes_Independent()
    {
        Guid firstId = Guid.NewGuid();
        Guid secondId;
        do
        {
            secondId = Guid.NewGuid();
        }
        while (ObjectWriteLocks.GetStripeIndex(secondId) == ObjectWriteLocks.GetStripeIndex(firstId));

        using IDisposable first = await ObjectWriteLocks.Acquire(firstId, CancellationToken.None);

        Task<IDisposable> second = ObjectWriteLocks.Acquire(secondId, CancellationToken.None).AsTask();
        using IDisposable secondLock = await second.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.IsTrue(second.IsCompleted);
    }

    [TestMethod]
    public async Task Acquire_DifferentIdsSameStripe_SerializeAndBothComplete()
    {
        Guid firstId = Guid.NewGuid();
        Guid secondId;
        do
        {
            secondId = Guid.NewGuid();
        }
        while (secondId == firstId || ObjectWriteLocks.GetStripeIndex(secondId) != ObjectWriteLocks.GetStripeIndex(firstId));

        IDisposable first = await ObjectWriteLocks.Acquire(firstId, CancellationToken.None);

        Task<IDisposable> second = ObjectWriteLocks.Acquire(secondId, CancellationToken.None).AsTask();
        await Task.Delay(100);
        Assert.IsFalse(second.IsCompleted);

        first.Dispose();
        IDisposable secondLock = await second.WaitAsync(TimeSpan.FromSeconds(10));
        secondLock.Dispose();

        // The stripe is free again.
        using IDisposable third = await ObjectWriteLocks.Acquire(firstId, CancellationToken.None).AsTask().WaitAsync(TimeSpan.FromSeconds(10));
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
}
