using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;

namespace BouncyHsm.Core.Tests.Services.Contracts.Generators;

[TestClass]
public class Sp800_108DoublePipelineKdfTests
{
    public required TestContext TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void Sp800_108DoublePipelineKdf_ShortResult_Success()
    {
        byte[] key = Convert.FromHexString("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d");
        Sp800_108DoublePipelineKdf kdf = new Sp800_108DoublePipelineKdf(() => new HMac(new Sha256Digest()),
            key);

        IPrfDataParam[] parameters = new IPrfDataParam[]
        {
            new IterationVariablePrfDataParam(false, 16 * 8),
            new ByteArrayPrfDataParam(Convert.FromHexString("99c3d79cb978724e1e2f09dc90e3b694")),
            new ByteArrayPrfDataParam(new byte[]{0x00}),
            new CounterPrfDataParam(true, 32),
            new DkmLengthPrfDataParam(false, 32, Core.Services.Contracts.P11.CK_SP800_108_DKM_LENGTH_METHOD.CK_SP800_108_DKM_LENGTH_SUM_OF_KEYS),
        };

        byte[] bytes = kdf.Derive(13, parameters);

        Assert.IsNotNull(bytes);
        Assert.HasCount(13, bytes);
    }

    [TestMethod]
    public void Sp800_108DoublePipelineKdf_MinimalSet_Success()
    {
        byte[] key = Convert.FromHexString("26ae34662efaac54fff373bf3ca5ec89b6db9532e9dc3158213c06a38616996d");
        Sp800_108DoublePipelineKdf kdf = new Sp800_108DoublePipelineKdf(() => new HMac(new Sha256Digest()),
            key);

        IPrfDataParam[] parameters = new IPrfDataParam[]
        {
            new IterationVariablePrfDataParam(false, 32 * 8),
        };

        byte[] bytes = kdf.Derive(13, parameters);

        Assert.IsNotNull(bytes);
        Assert.HasCount(13, bytes);
    }
}
