using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Crypto;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class VerifyWithRecoveryState : ISessionState
{
    private readonly ISignerWithRecovery signer;

    public VerifyWithRecoveryState(ISignerWithRecovery signer)
    {
        System.Diagnostics.Debug.Assert(signer != null);

        this.signer = signer;
    }

    public bool Verify(byte[] signature, [NotNullWhen(true)] out byte[]? recoveredMessage)
    {
        System.Diagnostics.Debug.Assert(signature != null);

        if( this.signer.VerifySignature(signature))
        {
            recoveredMessage = this.signer.GetRecoveredMessage();
            return true;
        }
        else
        {
            recoveredMessage = null;
            return false;
        }
    }

    public override string ToString()
    {
        return $"Verify with recovery state - algorithm: {this.signer.AlgorithmName}";
    }
}