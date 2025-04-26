using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI81.MechanismParams;

internal class CkSalsa20Params : ICkSalsa20Params
{
    private bool disposedValue;

    private CK_SALSA20_PARAMS lowLevelStruct = new CK_SALSA20_PARAMS();


    public CkSalsa20Params(ulong blockCounter, byte[] nonce)
    {
        this.lowLevelStruct.pBlockCounter = MemoryUtils.MemDup(ref blockCounter);
        this.lowLevelStruct.pNonce = MemoryUtils.MemDup(nonce);
        this.lowLevelStruct.ulNonceBits = (uint)nonce.Length * 8;
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

    ~CkSalsa20Params()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}
