using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal abstract class EncryptState : ISessionState
{
    protected readonly CKM mechanism;

    public bool IsUpdated
    {
        get;
        private set;
    }

    protected EncryptState(CKM mechanism)
    {
        this.mechanism = mechanism;
        this.IsUpdated = false;
    }

    public abstract uint GetUpdateSize(byte[] partData);

    public abstract uint GetFinalSize(byte[] data);

    public abstract uint GetFinalSize();

    public byte[] Update(byte[] partData)
    {
        try
        {
            byte[]? cipherText = this.UpdateInternal(partData);
            this.IsUpdated = true;

            return cipherText ?? Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            throw this.HandleError(ex);
        }
    }

    protected abstract byte[]? UpdateInternal(byte[] partData);

    public byte[] DoFinal(byte[] partData)
    {
        try
        {
            byte[]? cipherText = this.DoFinalInternal(partData);
            this.IsUpdated = false;

            return cipherText ?? Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            throw this.HandleError(ex);
        }
    }

    protected abstract byte[]? DoFinalInternal(byte[] partData);

    public byte[] DoFinal()
    {
        if (!this.IsUpdated)
        {
            throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Error: Cipher empty data.");
        }

        try
        {
            return this.DoFinalInternal() ?? Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            throw this.HandleError(ex);
        }
    }

    protected abstract byte[]? DoFinalInternal();

    private RpcPkcs11Exception HandleError(Exception ex)
    {
        if (ex is RpcPkcs11Exception pkcs11Ex)
        {
            return pkcs11Ex;
        }

        if (ex is Org.BouncyCastle.Crypto.DataLengthException)
        {
            return new RpcPkcs11Exception(CKR.CKR_DATA_LEN_RANGE, "Error: Data length range exceeded.", ex);
        }

        return new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Error: Decrypt operation failed.", ex);
    }
}
