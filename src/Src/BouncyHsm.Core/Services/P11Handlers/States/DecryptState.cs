using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class DecryptState : ISessionState
{
    private readonly IBufferedCipher bufferedCipher;
    private readonly CKM mechanism;

    public bool IsUpdated
    {
        get;
        private set;
    }

    public DecryptState(IBufferedCipher bufferedCipher, CKM mechanism)
    {
        this.bufferedCipher = bufferedCipher;
        this.mechanism = mechanism;
        this.IsUpdated = false;
    }

    public uint GetUpdateSize(byte[] partData)
    {
        return (uint)this.bufferedCipher.GetUpdateOutputSize(partData.Length);
    }

    public uint GetFinalSize(byte[] data)
    {
        return (uint)this.bufferedCipher.GetOutputSize(data.Length);
    }

    public uint GetFinalSize()
    {
        return (uint)this.bufferedCipher.GetOutputSize(0);
    }

    public byte[] Update(byte[] partData)
    {
        byte[]? plainText = this.bufferedCipher.ProcessBytes(partData);
        this.IsUpdated = true;

        return plainText ?? Array.Empty<byte>();
    }

    public byte[] DoFinal(byte[] partData)
    {
        byte[]? plainText = this.bufferedCipher.DoFinal(partData);
        this.IsUpdated = false;

        return plainText ?? Array.Empty<byte>();
    }

    public byte[] DoFinal()
    {
        if (!this.IsUpdated)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR, "Error: Decrypt empty data.");
        }

        return this.bufferedCipher.DoFinal() ?? Array.Empty<byte>();
    }

    public override string ToString()
    {
        return $"Decrypt state with {this.bufferedCipher.AlgorithmName} for mechanism {this.mechanism}.";
    }
}
