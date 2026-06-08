using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Bc;

//TODO: Omptimize and check algorithms
public class Sp800_108CounterKdf : Sp800_108
{
    public Sp800_108CounterKdf(Func<IMac> macFactory)
        : base(macFactory)
    {
    }

    public override byte[] Derive(byte[] key, byte[] label, byte[] context, int outputLengthBytes)
    {
        using MemoryStream result = new MemoryStream();

        int hLen;

        IMac mac = this.macFactory();
        hLen = mac.GetMacSize();


        int n = (int)Math.Ceiling((double)outputLengthBytes / hLen);
        byte[] fixedInput = this.BuildFixedInput(label, context, outputLengthBytes * 8);

        for (int i = 1; i <= n; i++)
        {
            using MemoryStream data = new MemoryStream();
            data.Write(this.UInt32BE((uint)i), 0, 4);
            data.Write(fixedInput, 0, fixedInput.Length);
            byte[] block = this.PRF(key, data.ToArray());
            result.Write(block, 0, block.Length);
        }

        byte[] output = result.ToArray();
        Array.Resize(ref output, outputLengthBytes);

        return output;
    }
}
