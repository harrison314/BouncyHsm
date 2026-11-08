using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;
using System.Text;

namespace BouncyHsm.Core.Tests.Services.P11Handlers;

[TestClass]
public class ObjectWriteLockHandlerTests
{
    private const uint SlotId = 4;
    private const uint SessionId = 11;
    private const uint ObjectHandle = 42;

    private static readonly AppIdentification AppId = new AppIdentification() { AppName = "test", AppNonce = "nonce", Pid = 1 };

    private static readonly Lazy<AsymmetricCipherKeyPair> hssKeyPair = new Lazy<AsymmetricCipherKeyPair>(() =>
    {
        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(
            new[] { new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8) },
            new SecureRandom()));
        return generator.GenerateKeyPair();
    });

    private static DataObject CreateTokenObject(Guid id, string label)
    {
        return new DataObject()
        {
            Id = id,
            CkaLabel = label,
            CkaValue = new byte[32],
            CkaToken = true,
            CkaPrivate = false
        };
    }

    private static HssPrivateKeyObject CreateHssTokenKey(Guid id, string label)
    {
        HssPrivateKeyObject key = new HssPrivateKeyObject();
        key.Id = id;
        key.SetPrivateKey(hssKeyPair.Value.Private);
        key.CkaLabel = label;
        key.CkaToken = true;
        key.CkaPrivate = false;
        return key;
    }

    private static Mock<IP11HwServices> CreateHwServices(Guid objectId, Mock<IPersistentRepository> persistence, out Mock<IMemorySession> memorySession)
    {
        Mock<IP11Session> session = new Mock<IP11Session>(MockBehavior.Strict);
        session.SetupGet(t => t.SlotId).Returns(SlotId);
        session.SetupGet(t => t.SessionId).Returns(SessionId);
        session.SetupGet(t => t.IsRwSession).Returns(true);
        session.Setup(t => t.TryLoadObject(objectId)).Returns((StorageObject?)null);

        IP11Session? sessionObject = session.Object;
        memorySession = new Mock<IMemorySession>(MockBehavior.Strict);
        memorySession.Setup(t => t.TryGetSession(SessionId, out sessionObject)).Returns(true);
        memorySession.Setup(t => t.FindObjectHandle(ObjectHandle)).Returns(objectId);
        memorySession.Setup(t => t.IsLogged(SessionId, It.IsAny<CKU>())).Returns(true);

        IMemorySession? memorySessionObject = memorySession.Object;
        Mock<IClientApplicationContext> appCtx = new Mock<IClientApplicationContext>(MockBehavior.Strict);
        appCtx.Setup(t => t.TryGetMemorySession(It.IsAny<string>(), out memorySessionObject)).Returns(true);

        persistence.Setup(t => t.GetSlot(SlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlotEntity()
            {
                Id = Guid.NewGuid(),
                SlotId = SlotId,
                Description = "slot",
                Token = new BouncyHsm.Core.Services.Contracts.Entities.TokenInfo() { Label = "token", SerialNumber = "1" }
            });

        Mock<IP11HwServices> hw = new Mock<IP11HwServices>(MockBehavior.Strict);
        hw.SetupGet(t => t.Time).Returns(TimeProvider.System);
        hw.SetupGet(t => t.ClientAppCtx).Returns(appCtx.Object);
        hw.SetupGet(t => t.Persistence).Returns(persistence.Object);

        return hw;
    }

    [TestMethod]
    public async Task SetAttributeValue_HssKey_WaitsForObjectLock_AndPersistsObjectReloadedInsideLock()
    {
        Guid objectId = Guid.NewGuid();
        HssPrivateKeyObject firstLoad = CreateHssTokenKey(objectId, "first");
        HssPrivateKeyObject secondLoad = CreateHssTokenKey(objectId, "second");

        Mock<IPersistentRepository> persistence = new Mock<IPersistentRepository>(MockBehavior.Strict);
        persistence.SetupSequence(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstLoad)
            .ReturnsAsync(secondLoad);

        StorageObject? persisted = null;
        persistence.Setup(t => t.UpdateObject(SlotId, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()))
            .Callback<uint, StorageObject, CancellationToken>((_, storageObject, _) => persisted = storageObject)
            .Returns(ValueTask.CompletedTask);

        Mock<IP11HwServices> hw = CreateHwServices(objectId, persistence, out _);
        SetAttributeValueHandler handler = new SetAttributeValueHandler(hw.Object, new NullLogger<SetAttributeValueHandler>());

        SetAttributeValueRequest request = new SetAttributeValueRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            ObjectHandle = ObjectHandle,
            Template = new AttrValueFromNative[]
            {
                new AttrValueFromNative()
                {
                    AttributeType = (uint)CKA.CKA_LABEL,
                    ValueTypeHint = NativeAttributeValue.AttrValueFromNativeTypeByteArray,
                    ValueRawBytes = Encoding.UTF8.GetBytes("updated")
                }
            }
        };

        IDisposable held = await ObjectWriteLocks.Acquire(objectId, CancellationToken.None);
        Task<SetAttributeValueEnvelope> call;
        try
        {
            call = handler.Handle(request, CancellationToken.None);
            await Task.Delay(100);
            Assert.IsFalse(call.IsCompleted, "SetAttributeValue must wait for the object lock.");
            persistence.Verify(t => t.UpdateObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            held.Dispose();
        }

        SetAttributeValueEnvelope envelope = await call.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual((uint)CKR.CKR_OK, envelope.Rv);
        Assert.AreSame(secondLoad, persisted, "The object reloaded inside the lock must be persisted.");
        Assert.AreEqual("updated", secondLoad.CkaLabel);
        Assert.AreEqual("first", firstLoad.CkaLabel);
        persistence.Verify(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task DestroyObject_HssKey_WaitsForObjectLock_AndDestroysObjectReloadedInsideLock()
    {
        Guid objectId = Guid.NewGuid();
        HssPrivateKeyObject firstLoad = CreateHssTokenKey(objectId, "first");
        HssPrivateKeyObject secondLoad = CreateHssTokenKey(objectId, "second");

        Mock<IPersistentRepository> persistence = new Mock<IPersistentRepository>(MockBehavior.Strict);
        persistence.SetupSequence(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstLoad)
            .ReturnsAsync(secondLoad);

        StorageObject? destroyed = null;
        persistence.Setup(t => t.DestroyObject(SlotId, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()))
            .Callback<uint, StorageObject, CancellationToken>((_, storageObject, _) => destroyed = storageObject)
            .Returns(ValueTask.CompletedTask);

        Mock<IP11HwServices> hw = CreateHwServices(objectId, persistence, out Mock<IMemorySession> memorySession);
        memorySession.Setup(t => t.DestroyObjectHandle(objectId));

        DestroyObjectHandler handler = new DestroyObjectHandler(hw.Object, new NullLoggerFactory(), new NullLogger<DestroyObjectHandler>());
        DestroyObjectRequest request = new DestroyObjectRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            ObjectHandle = ObjectHandle
        };

        IDisposable held = await ObjectWriteLocks.Acquire(objectId, CancellationToken.None);
        Task<DestroyObjectEnvelope> call;
        try
        {
            call = handler.Handle(request, CancellationToken.None);
            await Task.Delay(100);
            Assert.IsFalse(call.IsCompleted, "DestroyObject must wait for the object lock.");
            persistence.Verify(t => t.DestroyObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            held.Dispose();
        }

        DestroyObjectEnvelope envelope = await call.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual((uint)CKR.CKR_OK, envelope.Rv);
        Assert.AreSame(secondLoad, destroyed, "The object reloaded inside the lock must be destroyed.");
        memorySession.Verify(t => t.DestroyObjectHandle(objectId), Times.Once);
        persistence.Verify(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task SetAttributeValue_NonStatefulObject_DoesNotTakeObjectLock()
    {
        Guid objectId = Guid.NewGuid();
        DataObject storedObject = CreateTokenObject(objectId, "first");

        Mock<IPersistentRepository> persistence = new Mock<IPersistentRepository>(MockBehavior.Strict);
        persistence.Setup(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedObject);
        persistence.Setup(t => t.UpdateObject(SlotId, storedObject, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        Mock<IP11HwServices> hw = CreateHwServices(objectId, persistence, out _);
        SetAttributeValueHandler handler = new SetAttributeValueHandler(hw.Object, new NullLogger<SetAttributeValueHandler>());

        SetAttributeValueRequest request = new SetAttributeValueRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            ObjectHandle = ObjectHandle,
            Template = new AttrValueFromNative[]
            {
                new AttrValueFromNative()
                {
                    AttributeType = (uint)CKA.CKA_LABEL,
                    ValueTypeHint = NativeAttributeValue.AttrValueFromNativeTypeByteArray,
                    ValueRawBytes = Encoding.UTF8.GetBytes("updated")
                }
            }
        };

        using IDisposable held = await ObjectWriteLocks.Acquire(objectId, CancellationToken.None);
        SetAttributeValueEnvelope envelope = await handler.Handle(request, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual((uint)CKR.CKR_OK, envelope.Rv);
        Assert.AreEqual("updated", storedObject.CkaLabel);
        persistence.Verify(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task DestroyObject_NonStatefulObject_DoesNotTakeObjectLock()
    {
        Guid objectId = Guid.NewGuid();
        DataObject storedObject = CreateTokenObject(objectId, "first");

        Mock<IPersistentRepository> persistence = new Mock<IPersistentRepository>(MockBehavior.Strict);
        persistence.Setup(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedObject);
        persistence.Setup(t => t.DestroyObject(SlotId, storedObject, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        Mock<IP11HwServices> hw = CreateHwServices(objectId, persistence, out Mock<IMemorySession> memorySession);
        memorySession.Setup(t => t.DestroyObjectHandle(objectId));

        DestroyObjectHandler handler = new DestroyObjectHandler(hw.Object, new NullLoggerFactory(), new NullLogger<DestroyObjectHandler>());
        DestroyObjectRequest request = new DestroyObjectRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            ObjectHandle = ObjectHandle
        };

        using IDisposable held = await ObjectWriteLocks.Acquire(objectId, CancellationToken.None);
        DestroyObjectEnvelope envelope = await handler.Handle(request, CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual((uint)CKR.CKR_OK, envelope.Rv);
        persistence.Verify(t => t.DestroyObject(SlotId, storedObject, It.IsAny<CancellationToken>()), Times.Once);
        persistence.Verify(t => t.TryLoadObject(SlotId, objectId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
