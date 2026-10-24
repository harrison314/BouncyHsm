using BouncyHsm.Core.Services.P11Handlers.Common;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.Security.Cryptography;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.Common;

[TestClass]
public class ChaChaEngineExtensionsTests
{
    [TestMethod]
    [DataRow(0U, 0U)]
    [DataRow(1U, 0U)]
    [DataRow(0U, 1U)]
    [DataRow(1U, 1U)]
    public void ChaChaEngineExtensions_Values_SetCounter(uint lower, uint upper)
    {
        byte[] key = RandomNumberGenerator.GetBytes(32);
        byte[] nonce = RandomNumberGenerator.GetBytes(8);
        ChaChaEngine chacha = new ChaChaEngine(20);

        chacha.Init(
            true,
            new ParametersWithIV(
                new KeyParameter(key),
                nonce));

        chacha.SetChaChaCounter(lower, upper);
        (uint obtrainLower, uint obtrainUpper) = chacha.GetChaChaCounter();

        Assert.AreEqual(lower, obtrainLower);
        Assert.AreEqual(upper, obtrainUpper);
    }

    [TestMethod]
    [DataRow(0U)]
    [DataRow(1U)]
    [DataRow(17U)]
    public void ChaChaEngineExtensions_ValueChaCha7539_SetCounter(uint counter)
    {
        byte[] key = RandomNumberGenerator.GetBytes(32);
        byte[] nonce = RandomNumberGenerator.GetBytes(8);
        ChaCha7539Engine chacha = new ChaCha7539Engine();

        chacha.Init(
            true,
            new ParametersWithIV(
                new KeyParameter(key),
                nonce));

        chacha.SetChaCha7539Counter(counter);
        uint obtrainedCounter = chacha.GetChaCha7539Counter();

        Assert.AreEqual(counter, obtrainedCounter);
    }
}