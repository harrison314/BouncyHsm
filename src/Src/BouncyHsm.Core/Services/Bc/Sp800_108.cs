using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Bc;

public abstract class Sp800_108
{
    protected readonly Func<IMac> macFactory;

    protected Sp800_108(Func<IMac> macFactory)
    {
        this.macFactory = macFactory;

    }

    public abstract byte[] Derive(byte[] key, byte[] label, byte[] context, int outputLengthBytes);

    protected byte[] PRF(byte[] key, byte[] data)
    {
        IMac mac = this.macFactory();
        mac.Init(new KeyParameter(key));
        mac.BlockUpdate(data, 0, data.Length);
        byte[] result = new byte[mac.GetMacSize()];
        mac.DoFinal(result, 0);
        return result;
    }

    protected byte[] UInt32BE(uint i)
    {
        return new byte[]
        {
            (byte)(i >> 24),
            (byte)(i >> 16),
            (byte)(i >> 8),
            (byte)(i)
        };
    }

    protected byte[] BuildFixedInput(byte[] label, byte[] context, int outputBits)
    {
        using MemoryStream ms = new MemoryStream();

        ms.Write(label, 0, label.Length);
        ms.WriteByte(0x00);
        ms.Write(context, 0, context.Length);
        ms.Write(this.UInt32BE((uint)outputBits), 0, 4);

        return ms.ToArray();
    }
}
