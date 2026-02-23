using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI41.MechanismParams;

internal class CkSignAdditionalContextParams : ICkSignAdditionalContextParams
{
    private bool disposedValue;

    private CK_SIGN_ADDITIONAL_CONTEXT lowLevelStruct = new CK_SIGN_ADDITIONAL_CONTEXT();

    public CkSignAdditionalContextParams(uint hedgeVariant, byte[]? context)
    {
        this.lowLevelStruct.hedgeVariant = hedgeVariant;
        this.lowLevelStruct.ulContextLen = 0;
        this.lowLevelStruct.pContext = IntPtr.Zero;
        if (context != null)
        {
            this.lowLevelStruct.ulContextLen = (uint)context.Length;
            this.lowLevelStruct.pContext = MemoryUtils.MemDup(context);
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

    ~CkSignAdditionalContextParams()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}