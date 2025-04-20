using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class DecryptStateWithStreamChipher : DecryptState
{
    private readonly IStreamCipher streamCipher;

    public DecryptStateWithStreamChipher(IStreamCipher streamCipher, CKM mechanism)
        : base(mechanism)
    {
        this.streamCipher = streamCipher;
    }

    public override uint GetFinalSize(byte[] data)
    {
        return (uint)data.Length;
    }

    public override uint GetFinalSize()
    {
        return 0;
    }

    public override uint GetUpdateSize(byte[] partData)
    {
        return (uint)partData.Length;
    }

    protected override byte[]? DoFinalInternal(byte[] partData)
    {
        if (partData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        byte[] output = new byte[partData.Length];
        this.streamCipher.ProcessBytes(partData.AsSpan(), output.AsSpan());
        return output;
    }

    protected override byte[]? DoFinalInternal()
    {
        return null;
    }

    protected override byte[]? UpdateInternal(byte[] partData)
    {
        if (partData.Length == 0)
        {
            return Array.Empty<byte>();
        }

        byte[] output = new byte[partData.Length];
        this.streamCipher.ProcessBytes(partData.AsSpan(), output.AsSpan());
        return output;
    }

    public override string ToString()
    {
        return $"Stream Decrypt state with {this.streamCipher.AlgorithmName} for mechanism {this.mechanism}.";
    }
}
