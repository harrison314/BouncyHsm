using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pqc.Crypto;

namespace BouncyHsm.Core.Services.Bc;

/// <summary>
/// Adapts a one-shot <see cref="IMessageSigner"/> to <see cref="ISigner"/> by buffering the message.
/// </summary>
internal class MessageSignerAdapter : ISigner
{
    private readonly IMessageSigner messageSigner;
    private readonly MemoryStream buffer;

    public string AlgorithmName
    {
        get;
    }

    public MessageSignerAdapter(IMessageSigner messageSigner, string algorithmName)
    {
        System.Diagnostics.Debug.Assert(messageSigner != null);
        System.Diagnostics.Debug.Assert(algorithmName != null);

        this.messageSigner = messageSigner;
        this.AlgorithmName = algorithmName;
        this.buffer = new MemoryStream();
    }

    public void Init(bool forSigning, ICipherParameters parameters)
    {
        this.messageSigner.Init(forSigning, parameters);
        this.Reset();
    }

    public void Update(byte input)
    {
        this.buffer.WriteByte(input);
    }

    public void BlockUpdate(byte[] input, int inOff, int inLen)
    {
        this.buffer.Write(input, inOff, inLen);
    }

    public void BlockUpdate(ReadOnlySpan<byte> input)
    {
        this.buffer.Write(input);
    }

    public int GetMaxSignatureSize()
    {
        return -1;
    }

    public byte[] GenerateSignature()
    {
        byte[] signature = this.messageSigner.GenerateSignature(this.buffer.ToArray());
        this.Reset();
        return signature;
    }

    public bool VerifySignature(byte[] signature)
    {
        bool result = this.messageSigner.VerifySignature(this.buffer.ToArray(), signature);
        this.Reset();
        return result;
    }

    public void Reset()
    {
        this.buffer.SetLength(0);
    }
}
