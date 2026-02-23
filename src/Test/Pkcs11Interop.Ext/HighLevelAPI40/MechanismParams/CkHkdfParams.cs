using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

internal class CkHkdfParams : ICkHkdfParams
{
    private bool disposedValue;

    private CK_HKDF_PARAMS lowLevelStruct = new CK_HKDF_PARAMS();


    public CkHkdfParams(bool extract,
        bool expand,
        CKM hashMechanism,
        uint saltType,
        IObjectHandle? saltKey,
        byte[]? salt,
        byte[]? info)
    {
        this.lowLevelStruct.bExtract = extract ? (byte)1 : (byte)0;
        this.lowLevelStruct.bExpand = expand ? (byte)1 : (byte)0;
        this.lowLevelStruct.prfHashMechanism = (uint)hashMechanism;
        this.lowLevelStruct.ulSaltType = (uint)saltType;

        this.lowLevelStruct.pSalt = IntPtr.Zero;
        this.lowLevelStruct.ulSaltLen = 0;

        this.lowLevelStruct.hSaltKey = 0;

        this.lowLevelStruct.pInfo = IntPtr.Zero;
        this.lowLevelStruct.ulInfoLen = 0;

        if (saltKey != null)
        {
            this.lowLevelStruct.hSaltKey = (uint)saltKey.ObjectId;
        }

        if (salt != null)
        {
            this.lowLevelStruct.pSalt = MemoryUtils.MemDup(salt);
            this.lowLevelStruct.ulSaltLen = (uint)salt.Length;
        }

        if (info != null)
        {
            this.lowLevelStruct.pInfo = MemoryUtils.MemDup(info);
            this.lowLevelStruct.ulInfoLen = (uint)info.Length;
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

            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pSalt);
            MemoryUtils.MemFreeReset(ref this.lowLevelStruct.pInfo);

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~CkHkdfParams()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }
}