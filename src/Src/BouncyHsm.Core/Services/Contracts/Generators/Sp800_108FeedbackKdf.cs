using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.Contracts.Generators;

// https://csrc.nist.gov/files/pubs/sp/800/108/final/docs/sp800-108-nov2008.pdf
internal class Sp800_108FeedbackKdf
{
    private readonly Func<IMac> macFactory;
    private readonly byte[] key;
    private readonly byte[] iv;

    public Sp800_108FeedbackKdf(Func<IMac> macFactory, byte[] key, byte[] iv)
    {
        this.macFactory = macFactory;
        this.key = key;
        this.iv = iv;
    }

    public byte[] Derive(int outputLengthBytes, IPrfDataParam[] prfDataParams)
    {
        using MemoryStream result = new MemoryStream();

        int hLen = this.macFactory().GetMacSize();

        int n = (outputLengthBytes - 1 + hLen) / hLen;
        byte[] buffer = new byte[hLen];
        byte[] acc = this.iv;

        for (int i = 1; i <= n; i++)
        {
            PrfDataContext ctx = new PrfDataContext(i, acc, outputLengthBytes, n * hLen);
            IMac mac = this.macFactory();
            mac.Init(new KeyParameter(this.key));

            foreach (IPrfDataParam dp in prfDataParams)
            {
                dp.Apply(mac, ref ctx);
            }

            mac.DoFinal(buffer.AsSpan());
            result.Write(buffer.AsSpan());
            acc = buffer;
        }

        byte[] resultArray = result.ToArray();
        if (resultArray.Length == outputLengthBytes)
        {
            return resultArray;
        }

        return resultArray[..outputLengthBytes];
    }
}
