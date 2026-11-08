namespace BouncyHsm.Core.Services.P11Handlers.Common;

/// <summary>
/// Signer for stateful signature schemes (e.g. HSS/LMS). Implementations must persist
/// the advanced key state before returning a signature.
/// </summary>
internal interface IStatefulSigner
{
    /// <summary>
    /// Exact signature length in bytes, known without signing (no one-time key is consumed).
    /// </summary>
    uint SignatureSize
    {
        get;
    }

    ValueTask<byte[]> Sign(byte[] data, CancellationToken cancellationToken);
}
