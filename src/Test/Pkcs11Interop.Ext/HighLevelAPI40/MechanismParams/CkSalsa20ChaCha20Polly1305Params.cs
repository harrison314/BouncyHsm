using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

internal class CkSalsa20ChaCha20Polly1305Params : ICkSalsa20ChaCha20Polly1305Params
{
    private bool disposedValue;

    private CK_SALSA20_CHACHA20_POLY1305_PARAMS lowLevelStruct = new CK_SALSA20_CHACHA20_POLY1305_PARAMS();

    public CkSalsa20ChaCha20Polly1305Params(byte[] nonce, byte[]? aadData)
    {
        this.lowLevelStruct.pNonce = MemoryUtils.MemDup(nonce);
        this.lowLevelStruct.ulNonceLen = (uint)nonce.Length;
        this.lowLevelStruct.pAAD = IntPtr.Zero;
        this.lowLevelStruct.ulAADLen = 0;

        if (aadData != null)
        {
            this.lowLevelStruct.pAAD = MemoryUtils.MemDup(aadData);
            this.lowLevelStruct.ulAADLen = (uint)aadData.Length;
        }
    }

    public object ToMarshalableStructure()
    {
        if (this.disposedValue)
            throw new ObjectDisposedException(this.GetType().FullName);

        return this.lowLevelStruct;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pNonce);
            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pAAD);

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~CkSalsa20ChaCha20Polly1305Params()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}
