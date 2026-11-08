using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.Common;

[TestClass]
public class HssStatefulSignerTests
{
    private const int SignatureLength = 1296;
    private const uint SlotId = 3;

    private sealed class FakeStore
    {
        private readonly object syncRoot = new object();
        private StorageObjectMemento? current;

        public int UpdateCount
        {
            get;
            private set;
        }

        public bool Deleted
        {
            get;
            set;
        }

        public FakeStore(StorageObject initial)
        {
            this.current = initial.ToMemento();
        }

        public async ValueTask<StorageObject?> LoadAsync()
        {
            // Yield so that unsynchronized callers would interleave.
            await Task.Delay(5);
            return this.Load();
        }

        public StorageObject? Load()
        {
            lock (this.syncRoot)
            {
                if (this.Deleted || this.current == null)
                {
                    return null;
                }

                return StorageObjectFactory.CreateFromMemento(this.current);
            }
        }

        public async ValueTask UpdateAsync(StorageObject storageObject)
        {
            await Task.Delay(5);
            this.Update(storageObject);
        }

        public void Update(StorageObject storageObject)
        {
            lock (this.syncRoot)
            {
                this.current = storageObject.ToMemento();
                this.UpdateCount++;
            }
        }

        public HssPrivateKeyObject Stored()
        {
            return (HssPrivateKeyObject)this.Load()!;
        }
    }

    private static HssPrivateKeyObject CreateKey(bool token, out HssPublicKeyParameters publicKey)
    {
        return CreateKey(token, 1, out publicKey);
    }

    private static HssPrivateKeyObject CreateKey(bool token, int levels, out HssPublicKeyParameters publicKey)
    {
        LmsParameters[] parameters = Enumerable.Range(0, levels)
            .Select(_ => new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8))
            .ToArray();

        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(parameters, new SecureRandom()));
        AsymmetricCipherKeyPair pair = generator.GenerateKeyPair();

        publicKey = (HssPublicKeyParameters)pair.Public;
        HssPrivateKeyObject obj = new HssPrivateKeyObject();
        obj.Id = Guid.NewGuid();
        obj.SetPrivateKey(pair.Private);
        obj.CkaToken = token;
        obj.CkaSign = true;
        return obj;
    }

    private static (Mock<IP11HwServices> hw, Mock<IPersistentRepository> persistence, Mock<IP11Session> session) CreateMocks(FakeStore store)
    {
        Mock<IPersistentRepository> persistence = new Mock<IPersistentRepository>(MockBehavior.Strict);
        persistence.Setup(t => t.TryLoadObject(SlotId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((uint _, Guid _, CancellationToken _) => store.LoadAsync());
        persistence.Setup(t => t.UpdateObject(SlotId, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()))
            .Returns((uint _, StorageObject so, CancellationToken _) => store.UpdateAsync(so));

        Mock<IP11HwServices> hw = new Mock<IP11HwServices>(MockBehavior.Strict);
        hw.SetupGet(t => t.Persistence).Returns(persistence.Object);

        Mock<IP11Session> session = new Mock<IP11Session>(MockBehavior.Strict);
        session.SetupGet(t => t.SlotId).Returns(SlotId);
        session.Setup(t => t.TryLoadObject(It.IsAny<Guid>())).Returns((Guid _) => store.Load());
        session.Setup(t => t.UpdateObject(It.IsAny<StorageObject>())).Callback((StorageObject so) => store.Update(so));

        return (hw, persistence, session);
    }

    private static bool Verify(HssPublicKeyParameters publicKey, byte[] data, byte[] signature)
    {
        HssSigner verifier = new HssSigner();
        verifier.Init(false, publicKey);
        return verifier.VerifySignature(data, signature);
    }

    private static uint LeafIndex(byte[] signature)
    {
        return ((uint)signature[4] << 24) | ((uint)signature[5] << 16) | ((uint)signature[6] << 8) | signature[7];
    }

    // RFC 8554 section 6.2 layout for L=2, lms_sha256_n32_h5 + sha256_n32_w8 on both levels:
    // u32 Nspk || LMS sig of level-1 pubkey (1292 bytes) || level-1 LMS pubkey (56 bytes) || LMS sig of message (1292 bytes).
    private const int TwoLevelSignatureLength = 4 + 1292 + 56 + 1292;
    private const int TwoLevelTopQOffset = 4;
    private const int TwoLevelLowerIOffset = 4 + 1292 + 4 + 4;
    private const int TwoLevelLowerQOffset = 4 + 1292 + 56;

    private static uint ReadU32(byte[] data, int offset)
    {
        return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
    }

    private static (uint topQ, string lowerI, uint lowerQ) TwoLevelOneTimeKey(byte[] signature)
    {
        Assert.AreEqual(TwoLevelSignatureLength, signature.Length);
        Assert.AreEqual(1u, ReadU32(signature, 0), "Nspk");
        return (ReadU32(signature, TwoLevelTopQOffset),
            Convert.ToHexString(signature, TwoLevelLowerIOffset, 16),
            ReadU32(signature, TwoLevelLowerQOffset));
    }

    [TestMethod]
    public async Task Sign_TokenKey_PersistsBeforeReturn_AndDecrementsRemaining()
    {
        HssPrivateKeyObject key = CreateKey(true, out HssPublicKeyParameters publicKey);
        FakeStore store = new FakeStore(key);
        var (hw, persistence, session) = CreateMocks(store);
        byte[] data = new byte[] { 1, 2, 3, 4 };

        HssStatefulSigner signer = new HssStatefulSigner(hw.Object, session.Object, key.Id, true, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);
        byte[] signature = await signer.Sign(data, CancellationToken.None);

        persistence.Verify(t => t.UpdateObject(SlotId, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Once);
        session.Verify(t => t.UpdateObject(It.IsAny<StorageObject>()), Times.Never);
        Assert.AreEqual(31u, store.Stored().CkaHssKeysRemaining);
        Assert.AreEqual(SignatureLength, signature.Length);
        Assert.AreEqual((uint)signature.Length, signer.SignatureSize);
        Assert.IsTrue(Verify(publicKey, data, signature));
    }

    [TestMethod]
    public async Task Sign_SessionKey_UpdatesSessionStore()
    {
        HssPrivateKeyObject key = CreateKey(false, out HssPublicKeyParameters publicKey);
        FakeStore store = new FakeStore(key);
        var (hw, persistence, session) = CreateMocks(store);
        byte[] data = new byte[] { 5, 6, 7 };

        HssStatefulSigner signer = new HssStatefulSigner(hw.Object, session.Object, key.Id, false, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);
        byte[] signature = await signer.Sign(data, CancellationToken.None);

        session.Verify(t => t.UpdateObject(It.IsAny<StorageObject>()), Times.Once);
        persistence.Verify(t => t.UpdateObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.AreEqual(31u, store.Stored().CkaHssKeysRemaining);
        Assert.IsTrue(Verify(publicKey, data, signature));
    }

    [TestMethod]
    public async Task Sign_TwoSigners_SameKey_UseDistinctIndexes()
    {
        HssPrivateKeyObject key = CreateKey(true, out HssPublicKeyParameters publicKey);
        FakeStore store = new FakeStore(key);
        var (hw, _, session) = CreateMocks(store);
        byte[] data = new byte[] { 9, 9, 9 };

        HssStatefulSigner a = new HssStatefulSigner(hw.Object, session.Object, key.Id, true, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);
        HssStatefulSigner b = new HssStatefulSigner(hw.Object, session.Object, key.Id, true, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);

        byte[][] signatures = await Task.WhenAll(
            Task.Run(async () => await a.Sign(data, CancellationToken.None)),
            Task.Run(async () => await b.Sign(data, CancellationToken.None)));

        Assert.IsTrue(Verify(publicKey, data, signatures[0]));
        Assert.IsTrue(Verify(publicKey, data, signatures[1]));
        Assert.AreNotEqual(LeafIndex(signatures[0]), LeafIndex(signatures[1]));
        Assert.AreEqual(30u, store.Stored().CkaHssKeysRemaining);
    }

    [TestMethod]
    public async Task Sign_33rdSignature_ThrowsKeyExhausted_NoPersist()
    {
        HssPrivateKeyObject key = CreateKey(false, out HssPublicKeyParameters publicKey);
        FakeStore store = new FakeStore(key);
        var (hw, _, session) = CreateMocks(store);
        byte[] data = new byte[] { 1 };

        HssStatefulSigner signer = new HssStatefulSigner(hw.Object, session.Object, key.Id, false, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);
        HashSet<uint> indexes = new HashSet<uint>();
        for (int i = 0; i < 32; i++)
        {
            byte[] signature = await signer.Sign(data, CancellationToken.None);
            Assert.IsTrue(indexes.Add(LeafIndex(signature)));
        }

        Assert.AreEqual(0u, store.Stored().CkaHssKeysRemaining);

        RpcPkcs11Exception ex = await Assert.ThrowsExactlyAsync<RpcPkcs11Exception>(async () => await signer.Sign(data, CancellationToken.None));
        Assert.AreEqual(CKR.CKR_KEY_EXHAUSTED, ex.ReturnValue);
        Assert.AreEqual(32, store.UpdateCount);
        session.Verify(t => t.UpdateObject(It.IsAny<StorageObject>()), Times.Exactly(32));
    }

    [TestMethod]
    public async Task Sign_PersistFails_ThrowsWithoutSignature()
    {
        HssPrivateKeyObject key = CreateKey(true, out _);
        FakeStore store = new FakeStore(key);
        var (hw, persistence, session) = CreateMocks(store);
        persistence.Setup(t => t.UpdateObject(SlotId, It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromException(new IOException("disk full")));

        HssStatefulSigner signer = new HssStatefulSigner(hw.Object, session.Object, key.Id, true, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);

        await Assert.ThrowsExactlyAsync<IOException>(async () => await signer.Sign(new byte[] { 1 }, CancellationToken.None));
        Assert.AreEqual(32u, store.Stored().CkaHssKeysRemaining);
    }

    [TestMethod]
    public async Task Sign_ObjectDeleted_ThrowsKeyHandleInvalid()
    {
        HssPrivateKeyObject key = CreateKey(true, out _);
        FakeStore store = new FakeStore(key);
        store.Deleted = true;
        var (hw, persistence, session) = CreateMocks(store);

        HssStatefulSigner signer = new HssStatefulSigner(hw.Object, session.Object, key.Id, true, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);

        RpcPkcs11Exception ex = await Assert.ThrowsExactlyAsync<RpcPkcs11Exception>(async () => await signer.Sign(new byte[] { 1 }, CancellationToken.None));
        Assert.AreEqual(CKR.CKR_KEY_HANDLE_INVALID, ex.ReturnValue);
        persistence.Verify(t => t.UpdateObject(It.IsAny<uint>(), It.IsAny<StorageObject>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Sign_TwoLevels_CrossesLowerTreeRollover_UsesDistinctOneTimeKeys()
    {
        HssPrivateKeyObject key = CreateKey(true, 2, out HssPublicKeyParameters publicKey);
        Assert.AreEqual(1024u, key.CkaHssKeysRemaining);
        FakeStore store = new FakeStore(key);
        var (hw, _, session) = CreateMocks(store);

        HssStatefulSigner signer = new HssStatefulSigner(hw.Object, session.Object, key.Id, true, HssUtils.GetSignatureLength(key.CkaHssLmsTypes, key.CkaHssLmotsTypes), NullLogger.Instance);
        HashSet<(uint, string, uint)> oneTimeKeys = new HashSet<(uint, string, uint)>();
        List<(uint topQ, string lowerI, uint lowerQ)> ordered = new List<(uint, string, uint)>();

        for (int i = 0; i < 40; i++)
        {
            byte[] data = BitConverter.GetBytes(i);
            byte[] signature = await signer.Sign(data, CancellationToken.None);

            Assert.IsTrue(Verify(publicKey, data, signature), $"Signature {i} does not verify.");
            (uint topQ, string lowerI, uint lowerQ) oneTimeKey = TwoLevelOneTimeKey(signature);
            Assert.IsTrue(oneTimeKeys.Add(oneTimeKey), $"Signature {i} reuses one-time key {oneTimeKey}.");
            ordered.Add(oneTimeKey);
            Assert.AreEqual((uint)(1024 - i - 1), store.Stored().CkaHssKeysRemaining);
        }

        // Sanity checks for the parsed offsets.
        Assert.AreEqual(0u, ordered[0].topQ);
        Assert.AreEqual(0u, ordered[0].lowerQ);
        Assert.AreEqual(31u, ordered[31].lowerQ);
        Assert.AreEqual(0u, ordered[31].topQ);
        Assert.AreEqual(1u, ordered[32].topQ);
        Assert.AreEqual(0u, ordered[32].lowerQ);
        Assert.AreNotEqual(ordered[31].lowerI, ordered[32].lowerI);
        Assert.AreEqual(ordered[0].lowerI, ordered[31].lowerI);

        Assert.AreEqual(984u, store.Stored().CkaHssKeysRemaining);
        Assert.AreEqual(40, store.UpdateCount);
    }
}
