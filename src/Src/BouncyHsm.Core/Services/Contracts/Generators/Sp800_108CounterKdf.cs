using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class Sp800_108CounterKdf
{
    private readonly Func<IMac> macFactory;
    private readonly byte[] key;

    public Sp800_108CounterKdf(Func<IMac> macFactory, byte[] key)
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

        for (int i = 1; i <= n; i++)
        {
            PrfDataContext ctx = new PrfDataContext(i, null, outputLengthBytes, n * hLen);
            IMac mac = this.macFactory();
            mac.Init(new KeyParameter(this.key));

            foreach (IPrfDataParam dp in prfDataParams)
            {
                dp.Apply(mac, ref ctx);
            }

            mac.DoFinal(buffer.AsSpan());

            result.Write(buffer.AsSpan());
        }

        byte[] resultArray = result.ToArray();
        if (resultArray.Length == outputLengthBytes)
        {
            return resultArray;
        }

        return resultArray[..outputLengthBytes];
    }
}
