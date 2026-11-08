using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal sealed class SignStateWithSigner : SignState
{
    private readonly ISigner signer;

    public SignStateWithSigner(ISigner signer, Guid privateKeyId, bool alwaysAuthenticated, bool isObjectPrivate, bool acceptEmptyData)
        : base(privateKeyId, alwaysAuthenticated, isObjectPrivate, acceptEmptyData)
    {
        System.Diagnostics.Debug.Assert(signer != null);

        this.signer = signer;
    }

    public override uint GetMaxSignatureSize()
    {
        return Convert.ToUInt32(this.signer.GetMaxSignatureSize());
    }

    protected override void UpdateInternal(byte[] data)
    {
        this.signer.BlockUpdate(data);
    }

    protected override ValueTask<byte[]> GenerateSignatureInternal(CancellationToken cancellationToken)
    {
        return new ValueTask<byte[]>(this.signer.GenerateSignature());
    }

    public override string ToString()
    {
        return $"Sign state - algorithm: {this.signer.AlgorithmName}";
    }
}
