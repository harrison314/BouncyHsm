using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI80;

internal class SessionExtensions : ISessionExtensions
{
    delegate NativeULong C_SessionCancelDelegate(NativeULong sessionId, NativeULong flags);

    private C_SessionCancelDelegate C_SessionCancel;

    public SessionExtensions(IntPtr lirarayhandle)
    {
        this.C_SessionCancel = this.GetDelegate<C_SessionCancelDelegate>(lirarayhandle, "C_SessionCancel");
    }

    public void SessionCancel(ISession session, uint CkfFlags)
    {
        NativeULong rvRaw = this.C_SessionCancel((NativeULong)session.SessionId, (NativeULong)CkfFlags);
        if ((CKR)rvRaw != CKR.CKR_OK)
        {
            throw new Pkcs11Exception("C_SessionCancel", (CKR)rvRaw);
        }
    }

    private TDelegate GetDelegate<TDelegate>(IntPtr lirarayhandle, string functionName)
        where TDelegate : Delegate
    {
        IntPtr procAddress = NativeLibrary.GetExport(lirarayhandle, functionName);
        if (procAddress == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Cannot get address of {functionName} function");
        }
        return Marshal.GetDelegateForFunctionPointer<TDelegate>(procAddress);
    }
}
