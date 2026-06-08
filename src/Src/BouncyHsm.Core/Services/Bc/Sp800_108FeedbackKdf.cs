using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Bc;

//TODO: Omptimize and check algorithms
public class Sp800_108FeedbackKdf : Sp800_108
{
    private readonly byte[]? _iv;

    public Sp800_108FeedbackKdf(Func<IMac> macFactory, byte[]? iv = null)
        : base(macFactory)
    {
        this._iv = iv;
    }

    public override byte[] Derive(byte[] key, byte[] label, byte[] context, int outputLengthBytes)
    {
        using MemoryStream result = new MemoryStream();

        int hLen;
        IMac mac = this.macFactory();
        hLen = mac.GetMacSize();

        int n = (int)Math.Ceiling((double)outputLengthBytes / hLen);
        byte[] fixedInput = this.BuildFixedInput(label, context, outputLengthBytes * 8);

        byte[] previous = this._iv != null && this._iv.Length > 0 ? this._iv : fixedInput;


        for (int i = 1; i <= n; i++)
        {
            using MemoryStream data = new MemoryStream();
            data.Write(previous, 0, previous.Length);
            data.Write(this.UInt32BE((uint)i), 0, 4);
            data.Write(fixedInput, 0, fixedInput.Length);

            byte[] block = this.PRF(key, data.ToArray());

            result.Write(block, 0, block.Length);
            previous = block;
        }

        byte[] output = result.ToArray();
        Array.Resize(ref output, outputLengthBytes);

        return output;
    }
}
