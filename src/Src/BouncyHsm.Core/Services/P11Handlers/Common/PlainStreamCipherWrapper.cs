using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class PlainStreamCipherWrapper : IWrapper
{
    private readonly IStreamCipher streamCipher;
    private bool forWrapping;

    public string AlgorithmName
    {
        get => this.streamCipher.AlgorithmName;
    }

    public PlainStreamCipherWrapper(IStreamCipher streamCipher)
    {
        this.streamCipher = streamCipher;
        this.forWrapping = false;
    }

    public void Init(bool forWrapping, ICipherParameters parameters)
    {
        this.streamCipher.Init(forWrapping, parameters);
        this.forWrapping = forWrapping;
    }

    public byte[] Unwrap(byte[] input, int inOff, int length)
    {
        if (this.forWrapping)
        {
            throw new InvalidOperationException("Cipher is not in unwrapping mode.");
        }

        this.streamCipher.Reset();
        byte[] output = new byte[length];
        this.streamCipher.ProcessBytes(input.AsSpan(inOff, length), output.AsSpan());

        return output;
    }

    public byte[] Wrap(byte[] input, int inOff, int length)
    {
        if (!this.forWrapping)
        {
            throw new InvalidOperationException("Cipher is not in wrapping mode.");
        }

        this.streamCipher.Reset();
        byte[] output = new byte[length];
        this.streamCipher.ProcessBytes(input.AsSpan(inOff, length), output.AsSpan());

        return output;
    }
}
