using BouncyHsm.Core.Services.P11Handlers.Common;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BouncyHsm.Core.Tests.Services.P11Handlers.Common;

[TestClass]
public class Salsa20EngineExtensionsTests
{
    [TestMethod]
    [DataRow(0UL)]
    [DataRow(1UL)]
    [DataRow(8685855665155411255UL)]
    public void Salsa20EngineExtensions_Values_SetCounter(ulong counter)
    {
        byte[] key = RandomNumberGenerator.GetBytes(32);
        byte[] nonce = RandomNumberGenerator.GetBytes(8);
        Salsa20Engine salsa = new Salsa20Engine();

        salsa.Init(
            true,
            new ParametersWithIV(
                new KeyParameter(key),
                nonce));

        salsa.SetCounter(counter);
        ulong obtrainCounter = salsa.GetCounter();

        Assert.AreEqual(counter, obtrainCounter);
    }
}
