using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Crypto;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class VerifyWithRecoveryState : ISessionState
{
    private readonly ISignerWithRecovery signer;
    private bool isEmpty;

    public VerifyWithRecoveryState(ISignerWithRecovery signer)
    {
        System.Diagnostics.Debug.Assert(signer != null);

        this.signer = signer;
        this.isEmpty = true;
    }

    public void Update(byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        this.signer.BlockUpdate(data);

        if (data.Length > 0)
        {
            this.isEmpty = false;
        }
    }

    public bool Verify(byte[] signature, [NotNullWhen(true)] out byte[]? recoveredMessage)
    {
        System.Diagnostics.Debug.Assert(signature != null);

        if (this.isEmpty)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR, "Error: Verify empty data.");
        }

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