using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

internal class CkChaCha20Params : ICkChaCha20Params
{
    private bool disposedValue;

    private CK_CHACHA20_PARAMS lowLevelStruct = new CK_CHACHA20_PARAMS();

    public uint BlockCounter
    {
        get => this.GetGlockCounter();
    }

    public CkChaCha20Params(uint blockCounter, byte[] nonce)
    {
        CkChaCha20ParamsGuard.CheckNonceBits(nonce);

        this.lowLevelStruct.pBlockCounter = MemoryUtils.MemDup(ref blockCounter);
        this.lowLevelStruct.blockCounterBits = 32;
        this.lowLevelStruct.pNonce = MemoryUtils.MemDup(nonce);
        this.lowLevelStruct.ulNonceBits = (uint)nonce.Length * 8;
    }

    public CkChaCha20Params(ulong blockCounter, byte[] nonce)
    {
        CkChaCha20ParamsGuard.CheckNonceBits(nonce);

        this.lowLevelStruct.pBlockCounter = MemoryUtils.MemDup(ref blockCounter);
        this.lowLevelStruct.blockCounterBits = 64;
        this.lowLevelStruct.pNonce = MemoryUtils.MemDup(nonce);
        this.lowLevelStruct.ulNonceBits = (uint)nonce.Length * 8;
    }

    public object ToMarshalableStructure()
    {
        if (this.disposedValue)
            throw new ObjectDisposedException(this.GetType().FullName);

        return this.lowLevelStruct;
    }

    private uint GetGlockCounter()
    {
        if (this.disposedValue)
            throw new ObjectDisposedException(this.GetType().FullName);

        if (this.lowLevelStruct.blockCounterBits == 32)
        {
            uint blockCounter = 0;
            MemoryUtils.CopyBack(this.lowLevelStruct.pBlockCounter, ref blockCounter);
            return blockCounter;
        }
        else if (this.lowLevelStruct.blockCounterBits == 64)
        {
            ulong blockCounter = 0;
            MemoryUtils.CopyBack(this.lowLevelStruct.pBlockCounter, ref blockCounter);
            return (uint)blockCounter;
        }
        else
        {
            throw new ArgumentException("Invalid block counter bits");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pBlockCounter);
            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pNonce);

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~CkChaCha20Params()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}
