using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class EncryptStateWithBufferedChipher : EncryptState
{
    private readonly IBufferedCipher bufferedCipher;

    public EncryptStateWithBufferedChipher(IBufferedCipher bufferedCipher, CKM mechanism)
        : base(mechanism)
    {
        this.bufferedCipher = bufferedCipher;
    }

    public override uint GetUpdateSize(byte[] partData)
    {
        return (uint)this.bufferedCipher.GetUpdateOutputSize(partData.Length);
    }

    public override uint GetFinalSize(byte[] data)
    {
        return (uint)this.bufferedCipher.GetOutputSize(data.Length);
    }

    public override uint GetFinalSize()
    {
        return (uint)this.bufferedCipher.GetOutputSize(0);
    }

    protected override byte[]? UpdateInternal(byte[] partData)
    {
        return this.bufferedCipher.ProcessBytes(partData);
    }

    protected override byte[]? DoFinalInternal(byte[] partData)
    {
        return this.bufferedCipher.DoFinal(partData);
    }

    protected override byte[]? DoFinalInternal()
    {
        return this.bufferedCipher.DoFinal();
    }

    public override string ToString()
    {
        return $"Buffered Encrypt state with {this.bufferedCipher.AlgorithmName} for mechanism {this.mechanism}.";
    }
}
