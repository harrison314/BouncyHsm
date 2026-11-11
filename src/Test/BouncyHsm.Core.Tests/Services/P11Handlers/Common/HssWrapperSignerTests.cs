using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.Common;

[TestClass]
public class HssWrapperSignerTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IntoStatefulSigner_PropagatesAlwaysAuthenticate(bool alwaysAuthenticate)
    {
        HssPrivateKeyObject key = CreateKey();
        key.CkaAlwaysAuthenticate = alwaysAuthenticate;

        HssWrapperSigner wrapper = new HssWrapperSigner(NullLogger<HssWrapperSigner>.Instance);
        AuthenticatedStatefulSigner result = wrapper.IntoStatefulSigner(key,
            new Mock<IMemorySession>(MockBehavior.Strict).Object,
            new Mock<IP11HwServices>(MockBehavior.Strict).Object,
            new Mock<IP11Session>(MockBehavior.Strict).Object);

        Assert.AreEqual(alwaysAuthenticate, result.RequireSignaturePin);
        Assert.IsInstanceOfType<HssStatefulSigner>(result.Signer);
    }

    private static HssPrivateKeyObject CreateKey()
    {
        HssKeyPairGenerator generator = new HssKeyPairGenerator();
        generator.Init(new HssKeyGenerationParameters(
            new[] { new LmsParameters(LMSigParameters.lms_sha256_n32_h5, LMOtsParameters.sha256_n32_w8) },
            new SecureRandom()));
        AsymmetricCipherKeyPair pair = generator.GenerateKeyPair();

        HssPrivateKeyObject obj = new HssPrivateKeyObject();
        obj.Id = Guid.NewGuid();
        obj.SetPrivateKey(pair.Private);
        obj.CkaToken = true;
        obj.CkaSign = true;
        return obj;
    }
}
