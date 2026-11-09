using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.P11Handlers.States;

/// <summary>
/// Sign state for stateful signature schemes (e.g. HSS). The data is buffered and the
/// signature is created in one call, so no one-time key is consumed before C_Sign.
/// </summary>
internal sealed class SignStateWithStatefulSigner : SignState, IDisposable
{
    private readonly IStatefulSigner statefulSigner;
    private readonly MemoryStream buffer;
    private bool isDisposed;

    public override bool IsSinglePartOnly
    {
        get => true;
    }

    public SignStateWithStatefulSigner(IStatefulSigner statefulSigner, Guid privateKeyId, bool alwaysAuthenticated, bool isObjectPrivate)
        : base(privateKeyId, alwaysAuthenticated, isObjectPrivate, acceptEmptyData: true)
    {
        System.Diagnostics.Debug.Assert(statefulSigner != null);

        this.statefulSigner = statefulSigner;
        this.buffer = new MemoryStream();
        this.isDisposed = false;
    }

    public override uint GetMaxSignatureSize()
    {
        System.Diagnostics.Debug.Assert(this.isDisposed == false, "Object is disposed.");

        return this.statefulSigner.SignatureSize;
    }

    protected override void UpdateInternal(byte[] data)
    {
        System.Diagnostics.Debug.Assert(this.isDisposed == false, "Object is disposed.");

        this.buffer.Write(data);
    }

    protected override ValueTask<byte[]> GenerateSignatureInternal(CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.Assert(this.isDisposed == false, "Object is disposed.");

        return this.statefulSigner.Sign(this.buffer.ToArray(), cancellationToken);
    }

    public override string ToString()
    {
        return $"Sign state - stateful signer: {this.statefulSigner.GetType().Name}";
    }

    public void Dispose()
    {
        if (!this.isDisposed)
        {
            this.buffer.Dispose();
            this.isDisposed = true;
        }
    }
}
