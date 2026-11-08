using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal abstract class SignState : ISessionStateWithAlwaysAuthenticated
{
    private readonly bool acceptEmptyData;
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

    /// <summary>
    /// When true, only C_Sign is supported (C_SignUpdate and C_SignFinal are rejected)
    /// and a failed C_Sign terminates the operation.
    /// </summary>
    public virtual bool IsSinglePartOnly
    {
        get => false;
    }

    protected SignState(Guid privateKeyId, bool alwaysAuthenticated, bool isObjectPrivate, bool acceptEmptyData)
    {
        System.Diagnostics.Debug.Assert(privateKeyId != Guid.Empty);

        this.PrivateKeyId = privateKeyId;
        this.RequireContextPin = alwaysAuthenticated;
        this.IsContextPinHasSet = false;
        this.isEmpty = true;
        this.signature = null;
        this.RequiredUserLogin = isObjectPrivate;
        this.acceptEmptyData = acceptEmptyData;
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

        if (this.signature != null)
        {
            //TODO: Check compliance with the specification: use C_SignUpdate followed by C_SignFinal.
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_GENERAL_ERROR,
                "Currently, the BouncyHsm implementation does not support calling C_SignUpdate after C_SignFinal.");
        }

        if (data.Length > 0)
        {
            this.UpdateInternal(data);
            this.isEmpty = false;
        }
    }

    public async ValueTask<byte[]> GetSignatureAsync(CancellationToken cancellationToken)
    {
        if (this.isEmpty && !this.acceptEmptyData)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_DATA_LEN_RANGE, "Error: Signing empty data.");
        }

        if (this.RequireContextPin && !this.IsContextPinHasSet)
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_USER_NOT_LOGGED_IN, "Error: CONTEXT_SPECIFIC login required.");
        }

        this.signature ??= await this.GenerateSignatureInternal(cancellationToken);

        return this.signature;
    }

    public abstract uint GetMaxSignatureSize();

    protected abstract void UpdateInternal(byte[] data);

    protected abstract ValueTask<byte[]> GenerateSignatureInternal(CancellationToken cancellationToken);
}
