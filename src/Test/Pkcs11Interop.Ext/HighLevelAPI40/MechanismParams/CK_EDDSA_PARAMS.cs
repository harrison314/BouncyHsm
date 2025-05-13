using System.Runtime.InteropServices;
using NativeULong = System.UInt32;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Unicode)]
internal struct CK_EDDSA_PARAMS
{
    public byte phFlag; // CK_BBOOL
    public NativeULong ulContextDataLen;
    public IntPtr pContextData;
}