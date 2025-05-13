using System.Runtime.InteropServices;
using NativeULong = System.UInt32;

namespace Pkcs11Interop.Ext.HighLevelAPI41.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
internal struct CK_EDDSA_PARAMS
{
    public byte phFlag; // CK_BBOOL
    public NativeULong ulContextDataLen;
    public IntPtr pContextData;
}