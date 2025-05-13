using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI81.MechanismParams;

internal class CkEddsaParams: ICkEddsaParams
{
    private bool disposedValue;

    private CK_EDDSA_PARAMS lowLevelStruct = new CK_EDDSA_PARAMS();

    public CkEddsaParams(bool phFlag, byte[]? contextData)
    {
        this.lowLevelStruct.phFlag = phFlag ? (byte)1 : (byte)0;
        this.lowLevelStruct.ulContextDataLen = 0;
        this.lowLevelStruct.pContextData = IntPtr.Zero;

        if (contextData != null)
        {
            this.lowLevelStruct.ulContextDataLen = (uint)contextData.Length;
            if (contextData.Length == 0)
            {
                this.lowLevelStruct.pContextData = MemoryUtils.MemDup(new byte[4]);
            }
            else
            {
                this.lowLevelStruct.pContextData = MemoryUtils.MemDup(contextData);
            }
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

            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pContextData);

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~CkEddsaParams()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}
