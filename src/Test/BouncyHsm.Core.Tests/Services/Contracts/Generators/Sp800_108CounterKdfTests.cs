using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Tests.Services.Contracts.Generators;

[TestClass]
public class Sp800_108CounterKdfTests
{
    public required TestContext TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Sp800_108CounterKdf_ShortResult_Success()
    {
        byte[] key = Convert.FromHexString("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d");
        Sp800_108CounterKdf kdf = new Sp800_108CounterKdf(() => new HMac(new Sha256Digest()),
            key);

        IPrfDataParam[] parameters = new IPrfDataParam[]
        {
            new IterationVariablePrfDataParam(false, 32),
            new ByteArrayPrfDataParam(Convert.FromHexString("99c3d79cb978724e1e2f09dc90e3b694")),
            new ByteArrayPrfDataParam(new byte[]{0x00}),
            new ByteArrayPrfDataParam(Convert.FromHexString("18582cd847d60455fb88924c9fd8fb63")),
            new DkmLengthPrfDataParam(false, 32, Core.Services.Contracts.P11.CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS),
        };

        byte[] bytes = kdf.Derive(13, parameters);

        Assert.IsNotNull(bytes);
        Assert.HasCount(13, bytes);
    }

    [TestMethod]
    public void Sp800_108CounterKdf_LongResult_Success()
    {
        byte[] key = Convert.FromHexString("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d");
        Sp800_108CounterKdf kdf = new Sp800_108CounterKdf(() => new HMac(new Sha256Digest()),
            key);

        IPrfDataParam[] parameters = new IPrfDataParam[]
        {
            new IterationVariablePrfDataParam(false, 32),
            new ByteArrayPrfDataParam(Convert.FromHexString("99c3d79cb978724e1e2f09dc90e3b694")),
            new ByteArrayPrfDataParam(new byte[]{0x00}),
            new ByteArrayPrfDataParam(Convert.FromHexString("18582cd847d60455fb88924c9fd8fb63")),
            new DkmLengthPrfDataParam(false, 32, Core.Services.Contracts.P11.CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS),
        };

        byte[] bytes = kdf.Derive(1254, parameters);

        Assert.IsNotNull(bytes);
        Assert.HasCount(1254, bytes);
    }

    [TestMethod]
    [DataRow("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d",
        "99c3d79cb978724e1e2f09dc90e3b694",
        "18582cd847d60455fb88924c9fd8fb63",
        13)]
    [DataRow("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d",
        "99c3d79cb978724e1e2f09dc90e3b694",
        "18582cd847d60455fb88924c9fd8fb63",
        256)]
    [DataRow("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d",
        "",
        "18582cd847d60455fb88924c9fd8fb63",
        32)]
    [DataRow("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d",
        "16996d",
        "",
        32)]
    [DataRow("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d",
        "aa",
        "bb",
        32)]
    [DataRow("26ae34662efaac54",
        "aa",
        "bb",
        32)]
    public void Sp800_108CounterKdf_MathchWithNet(string hexkey, string hesLabel, string hexContext, int length)
    {
        byte[] key = Convert.FromHexString(hexkey);
        byte[] label = Convert.FromHexString(hesLabel);
        byte[] context = Convert.FromHexString(hexContext);
        byte[] exceptedResult = System.Security.Cryptography.SP800108HmacCounterKdf.DeriveBytes(key, System.Security.Cryptography.HashAlgorithmName.SHA256,
            label,
            context,
            length);


        Sp800_108CounterKdf kdf = new Sp800_108CounterKdf(() => new HMac(new Sha256Digest()),
            key);

        IPrfDataParam[] parameters = new IPrfDataParam[]
        {
            new IterationVariablePrfDataParam(false, 32),
            new ByteArrayPrfDataParam(label),
            new ByteArrayPrfDataParam(new byte[]{0x00}),
            new ByteArrayPrfDataParam(context),
            new DkmLengthPrfDataParam(false, 32, Core.Services.Contracts.P11.CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS),
        };

        byte[] bytes = kdf.Derive(length, parameters);

        this.TestContext.WriteLine(Convert.ToHexString(exceptedResult));
        this.TestContext.WriteLine(Convert.ToHexString(bytes));

        Assert.IsTrue(exceptedResult.SequenceEqual(bytes));
    }
}
