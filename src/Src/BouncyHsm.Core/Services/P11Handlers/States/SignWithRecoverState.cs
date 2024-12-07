using BouncyHsm.Core.Services.Contracts;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class SignWithRecoverState : ISessionStateWithAlwaysAuthenticated
{
    private readonly ISignerWithRecovery signer;
    private bool isEmpty;
    private byte[]? signature;

    public bool RequireContextPin
    {
        get;
    }

    public bool IsContextPinHasSet
    {
        get;
        protected set;
    }

    public bool RequiredUserLogin
    {
        get;
    }

    public Guid PrivateKeyId
    {
        get;
    }

    public SignWithRecoverState(ISignerWithRecovery signer, Guid privateKeyId, bool alwaysAuthenticated, bool isObjectPrivate)
    {
        System.Diagnostics.Debug.Assert(signer != null);
        System.Diagnostics.Debug.Assert(privateKeyId != Guid.Empty);

        this.signer = signer;
        this.PrivateKeyId = privateKeyId;
        this.RequireContextPin = alwaysAuthenticated;
        this.IsContextPinHasSet = false;
        this.isEmpty = true;
        this.signature = null;
        this.RequiredUserLogin = isObjectPrivate;
    }

    public void ContextLogin()
    {
        if (!this.RequireContextPin)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR, "Error: CONTEXT_SPECIFIC login not required.");
        }

        if (this.IsContextPinHasSet)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_USER_ALREADY_LOGGED_IN, "Error: Already CONTEXT_SPECIFIC logged.");
        }

        this.IsContextPinHasSet = true;
    }

    public override string ToString()
    {
        return $"Sign with recover state - algorithm: {this.signer.AlgorithmName}";
    }
}