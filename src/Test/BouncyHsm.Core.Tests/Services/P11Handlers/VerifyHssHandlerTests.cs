using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.Services.P11Handlers;

[TestClass]
public class VerifyHssHandlerTests
{
    private const uint SlotId = 4;
    private const uint SessionId = 11;
    private const uint KeyHandle = 42;

    private static readonly AppIdentification AppId = new AppIdentification() { AppName = "test", AppNonce = "nonce", Pid = 1 };

    private static AsymmetricCipherKeyPair GenerateKeyPair()
    {
        Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator generator = new Org.BouncyCastle.Pqc.Crypto.Lms.HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(
            new[] { new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8) },
            new SecureRandom()));
        return generator.GenerateKeyPair();
    }

    private static byte[] SignWithBc(AsymmetricKeyParameter privateKey, byte[] data)
    {
        HssSigner signer = new HssSigner();
        signer.Init(true, privateKey);
        return signer.GenerateSignature(data);
    }

    private static HssPublicKeyObject CreatePublicKeyObject(AsymmetricKeyParameter publicKey)
    {
        HssPublicKeyObject publicKeyObject = new HssPublicKeyObject();
        publicKeyObject.SetPublicKey(publicKey);
        publicKeyObject.CkaVerify = true;
        return publicKeyObject;
    }

    private static Mock<IP11HwServices> CreateHwServices(ISessionState initialState, out Mock<IP11Session> session, StorageObject? keyObject = null)
    {
        session = new Mock<IP11Session>(MockBehavior.Strict);
        session.SetupGet(t => t.SlotId).Returns(SlotId);
        session.SetupGet(t => t.SessionId).Returns(SessionId);
        session.SetupProperty(t => t.State, initialState);

        Guid keyId = keyObject?.Id ?? Guid.NewGuid();

        IP11Session? sessionObject = session.Object;
        Mock<IMemorySession> memorySession = new Mock<IMemorySession>(MockBehavior.Strict);
        memorySession.Setup(t => t.TryGetSession(SessionId, out sessionObject)).Returns(true);
        memorySession.Setup(t => t.FindObjectHandle(KeyHandle)).Returns(keyId);
        IP11Session p11Session = session.Object;
        memorySession.Setup(t => t.TryLoadObject(p11Session, keyId)).Returns(keyObject);
        memorySession.Setup(t => t.IsLogged(It.IsNotNull<IP11Session>(), CKU.CKU_USER)).Returns(true);

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

        Mock<IP11HwServices> hw = new Mock<IP11HwServices>(MockBehavior.Strict);
        hw.SetupGet(t => t.Time).Returns(TimeProvider.System);
        hw.SetupGet(t => t.ClientAppCtx).Returns(appCtx.Object);
        hw.SetupGet(t => t.Persistence).Returns(persistence.Object);

        return hw;
    }

    private static VerifyState CreateHssVerifyState(AsymmetricKeyParameter publicKey, bool isSinglePartOnly)
    {
        HssWrapperSigner wrapper = new HssWrapperSigner(new NullLogger<HssWrapperSigner>());
        ISigner signer = wrapper.IntoValidationSigner(CreatePublicKeyObject(publicKey));
        return new VerifyState(signer, wrapper.AcceptEmptyData, isSinglePartOnly);
    }

    [TestMethod]
    public async Task VerifyInit_Hss_CreatesSinglePartOnlyState()
    {
        AsymmetricCipherKeyPair keyPair = GenerateKeyPair();
        HssPublicKeyObject publicKeyObject = CreatePublicKeyObject(keyPair.Public);
        Mock<IP11HwServices> hw = CreateHwServices(EmptySessionState.Instance, out Mock<IP11Session> session, publicKeyObject);

        VerifyInitHandler handler = new VerifyInitHandler(hw.Object, new NullLoggerFactory(), new NullLogger<VerifyInitHandler>());
        VerifyInitEnvelope envelope = await handler.Handle(new VerifyInitRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            KeyObjectHandle = KeyHandle,
            Mechanism = new MechanismValue() { MechanismType = (uint)CKM.CKM_HSS }
        }, CancellationToken.None);

        Assert.AreEqual((uint)CKR.CKR_OK, envelope.Rv);
        VerifyState state = Assert.IsInstanceOfType<VerifyState>(session.Object.State);
        Assert.IsTrue(state.IsSinglePartOnly);
    }

    [TestMethod]
    public void VerifyState_DefaultMechanism_IsNotSinglePartOnly()
    {
        AsymmetricCipherKeyPair keyPair = GenerateKeyPair();
        VerifyState state = CreateHssVerifyState(keyPair.Public, false);

        Assert.IsFalse(state.IsSinglePartOnly);
    }

    [TestMethod]
    public async Task VerifyUpdate_SinglePartOnly_ReturnsFunctionNotSupportedAndClearsState()
    {
        AsymmetricCipherKeyPair keyPair = GenerateKeyPair();
        VerifyState state = CreateHssVerifyState(keyPair.Public, true);
        Mock<IP11HwServices> hw = CreateHwServices(state, out Mock<IP11Session> session);

        VerifyUpdateHandler handler = new VerifyUpdateHandler(hw.Object, new NullLogger<VerifyUpdateHandler>());
        RpcPkcs11Exception ex = await Assert.ThrowsExactlyAsync<RpcPkcs11Exception>(async () => await handler.Handle(new VerifyUpdateRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            Data = new byte[] { 1, 2, 3 }
        }, CancellationToken.None));

        Assert.AreEqual(CKR.CKR_FUNCTION_NOT_SUPPORTED, ex.ReturnValue);
        Assert.IsNotInstanceOfType<VerifyState>(session.Object.State);
    }

    [TestMethod]
    public async Task VerifyFinal_SinglePartOnly_ReturnsFunctionNotSupportedAndClearsState()
    {
        AsymmetricCipherKeyPair keyPair = GenerateKeyPair();
        byte[] data = new byte[] { 1, 2, 3 };
        byte[] signature = SignWithBc(keyPair.Private, data);
        VerifyState state = CreateHssVerifyState(keyPair.Public, true);
        Mock<IP11HwServices> hw = CreateHwServices(state, out Mock<IP11Session> session);

        VerifyFinalHandler handler = new VerifyFinalHandler(hw.Object, new NullLogger<VerifyFinalHandler>());
        RpcPkcs11Exception ex = await Assert.ThrowsExactlyAsync<RpcPkcs11Exception>(async () => await handler.Handle(new VerifyFinalRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            Signature = signature
        }, CancellationToken.None));

        Assert.AreEqual(CKR.CKR_FUNCTION_NOT_SUPPORTED, ex.ReturnValue);
        Assert.IsNotInstanceOfType<VerifyState>(session.Object.State);
    }

    [TestMethod]
    public async Task Verify_SinglePartOnly_VerifiesSignature()
    {
        AsymmetricCipherKeyPair keyPair = GenerateKeyPair();
        byte[] data = new byte[] { 1, 2, 3 };
        byte[] signature = SignWithBc(keyPair.Private, data);
        VerifyState state = CreateHssVerifyState(keyPair.Public, true);
        Mock<IP11HwServices> hw = CreateHwServices(state, out Mock<IP11Session> session);

        VerifyHandler handler = new VerifyHandler(hw.Object, new NullLogger<VerifyHandler>());
        VerifyEnvelope envelope = await handler.Handle(new VerifyRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            Data = data,
            Signature = signature
        }, CancellationToken.None);

        Assert.AreEqual((uint)CKR.CKR_OK, envelope.Rv);
        Assert.IsNotInstanceOfType<VerifyState>(session.Object.State);
    }

    [TestMethod]
    public async Task VerifyUpdateAndFinal_NotSinglePartOnly_StillWork()
    {
        AsymmetricCipherKeyPair keyPair = GenerateKeyPair();
        byte[] data = new byte[] { 1, 2, 3 };
        byte[] signature = SignWithBc(keyPair.Private, data);
        VerifyState state = CreateHssVerifyState(keyPair.Public, false);
        Mock<IP11HwServices> hw = CreateHwServices(state, out Mock<IP11Session> session);

        VerifyUpdateHandler updateHandler = new VerifyUpdateHandler(hw.Object, new NullLogger<VerifyUpdateHandler>());
        VerifyUpdateEnvelope updateEnvelope = await updateHandler.Handle(new VerifyUpdateRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            Data = data
        }, CancellationToken.None);
        Assert.AreEqual((uint)CKR.CKR_OK, updateEnvelope.Rv);

        VerifyFinalHandler finalHandler = new VerifyFinalHandler(hw.Object, new NullLogger<VerifyFinalHandler>());
        VerifyFinalEnvelope finalEnvelope = await finalHandler.Handle(new VerifyFinalRequest()
        {
            AppId = AppId,
            SessionId = SessionId,
            Signature = signature
        }, CancellationToken.None);

        Assert.AreEqual((uint)CKR.CKR_OK, finalEnvelope.Rv);
    }
}
