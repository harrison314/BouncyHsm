using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.Services.P11Handlers;

[TestClass]
public class SignHandlerTests
{
    private const uint SlotId = 4;
    private const uint SessionId = 11;

    private sealed class ThrowingStatefulSigner : IStatefulSigner
    {
        public int Calls
        {
            get;
            private set;
        }

        public uint SignatureSize
        {
            get => 1296;
        }

        public ValueTask<byte[]> Sign(byte[] data, CancellationToken cancellationToken)
        {
            this.Calls++;
            throw new RpcPkcs11Exception(CKR.CKR_KEY_EXHAUSTED, "HSS key exhausted.");
        }
    }

    private static (SignHandler handler, Mock<IP11Session> session) CreateHandler(ISessionState initialState)
    {
        Mock<IP11Session> session = new Mock<IP11Session>(MockBehavior.Strict);
        session.SetupGet(t => t.SlotId).Returns(SlotId);
        session.SetupProperty(t => t.State, initialState);

        IP11Session? sessionObject = session.Object;
        Mock<IMemorySession> memorySession = new Mock<IMemorySession>(MockBehavior.Strict);
        memorySession.Setup(t => t.TryGetSession(SessionId, out sessionObject)).Returns(true);
        memorySession.Setup(t => t.IsLogged(sessionObject, CKU.CKU_USER)).Returns(true);

        IMemorySession? memorySessionObject = memorySession.Object;
        Mock<IClientApplicationContext> appCtx = new Mock<IClientApplicationContext>(MockBehavior.Strict);
        appCtx.Setup(t => t.TryGetMemorySession(It.IsAny<string>(), out memorySessionObject)).Returns(true);

        Mock<IPersistentRepository> persistence = new Mock<IPersistentRepository>(MockBehavior.Strict);
        persistence.Setup(t => t.GetSlot(SlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SlotEntity()
            {
                Id = Guid.NewGuid(),
                SlotId = SlotId,
                Description = "slot",
                Token = new BouncyHsm.Core.Services.Contracts.Entities.TokenInfo() { Label = "token", SerialNumber = "1" }
            });
        persistence.Setup(t => t.TryLoadObject(SlotId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BouncyHsm.Core.Services.Contracts.Entities.HssPrivateKeyObject());

        Mock<IP11HwServices> hw = new Mock<IP11HwServices>(MockBehavior.Strict);
        hw.SetupGet(t => t.Time).Returns(TimeProvider.System);
        hw.SetupGet(t => t.ClientAppCtx).Returns(appCtx.Object);
        hw.SetupGet(t => t.Persistence).Returns(persistence.Object);

        SignHandler handler = new SignHandler(hw.Object, new NullLoggerFactory(), new NullLogger<SignHandler>());
        return (handler, session);
    }

    private static SignRequest CreateRequest(byte[] data)
    {
        return new SignRequest()
        {
            AppId = new AppIdentification() { AppName = "test", AppNonce = "nonce", Pid = 1 },
            SessionId = SessionId,
            Data = data,
            IsSignaturePtrSet = true,
            PullSignatureLen = 4096
        };
    }

    [TestMethod]
    public async Task Handle_StatefulSignerThrows_ClearsSignState()
    {
        ThrowingStatefulSigner statefulSigner = new ThrowingStatefulSigner();
        SignState state = new SignStateWithStatefulSigner(statefulSigner, Guid.NewGuid(), false, false);
        var (handler, session) = CreateHandler(state);

        RpcPkcs11Exception ex = await Assert.ThrowsExactlyAsync<RpcPkcs11Exception>(
            async () => await handler.Handle(CreateRequest(new byte[] { 1, 2, 3 }), CancellationToken.None));

        Assert.AreEqual(CKR.CKR_KEY_EXHAUSTED, ex.ReturnValue);
        Assert.AreEqual(1, statefulSigner.Calls);
        Assert.IsNotInstanceOfType<SignState>(session.Object.State);
    }

    [TestMethod]
    public async Task Handle_NonStatefulSignerThrows_KeepsSignState()
    {
        Org.BouncyCastle.Crypto.Signers.Ed25519Signer signer = new Org.BouncyCastle.Crypto.Signers.Ed25519Signer();
        signer.Init(true, new Org.BouncyCastle.Crypto.Parameters.Ed25519PrivateKeyParameters(new Org.BouncyCastle.Security.SecureRandom()));
        SignState state = new SignStateWithSigner(signer, Guid.NewGuid(), false, false, false);
        var (handler, session) = CreateHandler(state);

        RpcPkcs11Exception ex = await Assert.ThrowsExactlyAsync<RpcPkcs11Exception>(
            async () => await handler.Handle(CreateRequest(Array.Empty<byte>()), CancellationToken.None));

        Assert.AreEqual(CKR.CKR_DATA_LEN_RANGE, ex.ReturnValue);
        Assert.AreSame(state, session.Object.State);
    }
}
