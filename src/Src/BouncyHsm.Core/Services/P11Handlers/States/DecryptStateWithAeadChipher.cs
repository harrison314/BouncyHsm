using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto.Modes;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class DecryptStateWithAeadChipher : DecryptState
{
    private readonly IAeadCipher aeadCipher;

    public DecryptStateWithAeadChipher(IAeadCipher aeadCipher, CKM mechanism)
        : base(mechanism)
    {
        this.aeadCipher = aeadCipher;
    }

    public override uint GetFinalSize(byte[] data)
    {
        return (uint)this.aeadCipher.GetOutputSize(data.Length);
    }

    public override uint GetFinalSize()
    {
        return (uint)this.aeadCipher.GetOutputSize(0);
    }

    public override uint GetUpdateSize(byte[] partData)
    {
        return (uint)this.aeadCipher.GetUpdateOutputSize(partData.Length);
    }

    protected override byte[]? DoFinalInternal(byte[] partData)
    {
        byte[] output = new byte[this.aeadCipher.GetOutputSize(partData.Length)];
        int witedbytes = this.aeadCipher.ProcessBytes(partData.AsSpan(), output.AsSpan());
        int doFinalbytes = this.aeadCipher.DoFinal(output.AsSpan(witedbytes));

        return output;
    }

    protected override byte[]? DoFinalInternal()
    {
        byte[] output = new byte[this.aeadCipher.GetOutputSize(0)];
        this.aeadCipher.DoFinal(output.AsSpan());
        return output;
    }

    protected override byte[]? UpdateInternal(byte[] partData)
    {
        byte[] output = new byte[this.aeadCipher.GetUpdateOutputSize(partData.Length)];
        this.aeadCipher.ProcessBytes(partData.AsSpan(), output.AsSpan());
        return output;
    }

    public override string ToString()
    {
        return $"Aead decrypt state with {this.aeadCipher.AlgorithmName} for mechanism {this.mechanism}.";
    }
}