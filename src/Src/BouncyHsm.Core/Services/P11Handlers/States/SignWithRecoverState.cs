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

    public void Update(byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        if (this.RequireContextPin && !this.IsContextPinHasSet)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_USER_NOT_LOGGED_IN, "Error: CONTEXT_SPECIFIC login required.");
        }

        if (data.Length > 0)
        {
            this.signer.BlockUpdate(data);
            this.isEmpty = false;
        }
    }

    public byte[] GetSignature()
    {
        if (this.isEmpty)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR, "Error: Signing empty data.");
        }

        if (this.RequireContextPin && !this.IsContextPinHasSet)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_USER_NOT_LOGGED_IN, "Error: CONTEXT_SPECIFIC login required.");
        }

        if (this.signature == null)
        {
            this.signature = this.signer.GenerateSignature();
        }

        return this.signature;
    }

    public override string ToString()
    {
        return $"Sign with recover state - algorithm: {this.signer.AlgorithmName}";
    }
}