using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

internal class CkHashSignAdditionalContextParams : ICkHashSignAdditionalContextParams
{
    private bool disposedValue;

    private CK_HASH_SIGN_ADDITIONAL_CONTEXT lowLevelStruct = new CK_HASH_SIGN_ADDITIONAL_CONTEXT();

    public CkHashSignAdditionalContextParams(uint hedgeVariant, byte[]? context, uint hashs)
    {
        this.lowLevelStruct.hedgeVariant = hedgeVariant;
        this.lowLevelStruct.ulContextLen = 0;
        this.lowLevelStruct.pContext = IntPtr.Zero;
        if (context != null)
        {
            this.lowLevelStruct.ulContextLen = (uint)context.Length;
            this.lowLevelStruct.pContext = MemoryUtils.MemDup(context);
        }

        this.lowLevelStruct.hash = hashs;
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

            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pContext);

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~CkHashSignAdditionalContextParams()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}