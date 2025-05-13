using System.Runtime.InteropServices;
using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI81.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
internal struct CK_EDDSA_PARAMS
{
    public byte phFlag; // CK_BBOOL
    public NativeULong ulContextDataLen;
    public IntPtr pContextData;
}