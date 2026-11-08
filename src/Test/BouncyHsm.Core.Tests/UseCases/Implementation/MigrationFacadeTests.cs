using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.UseCases.Implementation;

[TestClass]
public class MigrationFacadeTests
{
    private const uint SlotId = 7;

    private static SlotEntity CreateSlot()
    {
        return new SlotEntity()
        {
            Description = "description",
            Id = Guid.NewGuid(),
            SlotId = SlotId,
            Token = new TokenInfo()
            {
                Label = "Label",
                SerialNumber = "000011"
            }
        };
    }

    // Returns the same key twice: before and after one signature.
    private static (HssPrivateKeyObject stale, HssPrivateKeyObject fresh) CreateHssKeyStates()
    {
        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(
            new[] { new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8) },
            new SecureRandom()));
        AsymmetricCipherKeyPair pair = generator.GenerateKeyPair();

        Guid id = Guid.NewGuid();
        HssPrivateKeyObject stale = new HssPrivateKeyObject();
        stale.Id = id;
        stale.CkaLabel = "hss";
        stale.SetPrivateKey(pair.Private);

        HssSigner signer = new HssSigner();
        signer.Init(true, pair.Private);
        signer.GenerateSignature(new byte[] { 1 });

        HssPrivateKeyObject fresh = new HssPrivateKeyObject();
        fresh.Id = id;
        fresh.CkaLabel = "hss";
        fresh.SetPrivateKey(pair.Private);

        Assert.AreEqual(32u, stale.CkaHssKeysRemaining);
        Assert.AreEqual(31u, fresh.CkaHssKeysRemaining);
        return (stale, fresh);
    }

    private static (Mock<IP11HwServices> hw, Mock<IPersistentRepository> repository) CreateMocks(StorageObject found, StorageObject? reloaded)
    {
        Mock<IPersistentRepository> repository = new Mock<IPersistentRepository>(MockBehavior.Strict);
        repository.Setup(t => t.GetSlots(It.IsAny<GetSlotSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SlotEntity>() { CreateSlot() });
        repository.Setup(t => t.ExecuteSlotCommand(SlotId, It.IsAny<IPersistentRepositorySlotCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        repository.Setup(t => t.FindObjects(SlotId, It.IsAny<FindObjectSpecification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StorageObject>() { found });
        repository.Setup(t => t.TryLoadObject(SlotId, found.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloaded);
        repository.Setup(t => t.UpdateObject(SlotId, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        Mock<IP11HwServices> hw = new Mock<IP11HwServices>(MockBehavior.Strict);
        hw.SetupGet(t => t.Persistence).Returns(repository.Object);

        return (hw, repository);
    }

    [TestMethod]
    public async Task Migrate_UpdatesReloadedInstance_NotStaleFindResult()
    {
        (HssPrivateKeyObject stale, HssPrivateKeyObject fresh) = CreateHssKeyStates();
        var (hw, repository) = CreateMocks(stale, fresh);

        MigrationFacade facade = new MigrationFacade(hw.Object, new NullLogger<MigrationFacade>());
        DomainResult<MigrationResult> result = await facade.Migrate(new MigrationRequest(), default);

        MigrationResult value = result.AssertOkValue();
        Assert.AreEqual(1, value.SucceededObjects);
        Assert.AreEqual(0, value.FailedObjects);
        repository.Verify(t => t.TryLoadObject(SlotId, stale.Id, It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(t => t.UpdateObject(SlotId,
                It.Is<StorageObject>(so => ((HssPrivateKeyObject)so).CkaHssKeysRemaining == 31u
                    && ((HssPrivateKeyObject)so).CkaValue.SequenceEqual(fresh.CkaValue)),
                It.IsAny<CancellationToken>()),
            Times.Once);
        repository.Verify(t => t.UpdateObject(SlotId,
                It.Is<StorageObject>(so => ((HssPrivateKeyObject)so).CkaValue.SequenceEqual(stale.CkaValue)),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Migrate_ObjectDeletedBeforeLock_IsSkipped()
    {
        (HssPrivateKeyObject stale, _) = CreateHssKeyStates();
        var (hw, repository) = CreateMocks(stale, null);

        MigrationFacade facade = new MigrationFacade(hw.Object, new NullLogger<MigrationFacade>());
        DomainResult<MigrationResult> result = await facade.Migrate(new MigrationRequest(), default);

        MigrationResult value = result.AssertOkValue();
        Assert.AreEqual(0, value.SucceededObjects);
        Assert.AreEqual(0, value.FailedObjects);
        repository.Verify(t => t.UpdateObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Migrate_ObjectLocked_WaitsForLock()
    {
        (HssPrivateKeyObject stale, HssPrivateKeyObject fresh) = CreateHssKeyStates();
        var (hw, repository) = CreateMocks(stale, fresh);

        MigrationFacade facade = new MigrationFacade(hw.Object, new NullLogger<MigrationFacade>());

        IDisposable held = await ObjectWriteLocks.Acquire(stale.Id, default);
        Task<DomainResult<MigrationResult>> task = facade.Migrate(new MigrationRequest(), default).AsTask();
        await Task.Delay(100);
        Assert.IsFalse(task.IsCompleted);
        repository.Verify(t => t.UpdateObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never);

        held.Dispose();
        DomainResult<MigrationResult> result = await task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(1, result.AssertOkValue().SucceededObjects);
    }
}
