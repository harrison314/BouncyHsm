using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;

namespace BouncyHsm.Core.Services.Contracts.Generators;

// https://csrc.nist.gov/files/pubs/sp/800/108/final/docs/sp800-108-nov2008.pdf
internal class Sp800_108DoublePipelineKdf
{
    private readonly Func<IMac> macFactory;
    private readonly byte[] key;

    public Sp800_108DoublePipelineKdf(Func<IMac> macFactory, byte[] key)
    {
        this.macFactory = macFactory;
        this.key = key;
    }

    public byte[] Derive(int outputLengthBytes, IPrfDataParam[] prfDataParams)
    {
        using MemoryStream result = new MemoryStream();

        int hLen = this.macFactory().GetMacSize();

        int n = (outputLengthBytes - 1 + hLen) / hLen;
        byte[] buffer = new byte[hLen];
        byte[] acc = this.BuildK0(prfDataParams, outputLengthBytes, n * hLen);

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

    private byte[] BuildK0(IPrfDataParam[] prfDataParams, int outputLengthBytes, int nBytes)
    {
        PrfDataContext ctx = new PrfDataContext(0, null, outputLengthBytes, nBytes);
        IMac mac = this.macFactory();
        mac.Init(new KeyParameter(this.key));

        foreach (IPrfDataParam dp in prfDataParams.Where(t=>t.Type != P11.CK_PRF_DATA_TYPE.CK_SP800_108_COUNTER
            && t.Type != P11.CK_PRF_DATA_TYPE.CK_SP800_108_ITERATION_VARIABLE))
        {
            dp.Apply(mac, ref ctx);
        }

        byte[] buffer = new byte[mac.GetMacSize()];
        mac.DoFinal(buffer.AsSpan());

        return buffer;
    }
}
