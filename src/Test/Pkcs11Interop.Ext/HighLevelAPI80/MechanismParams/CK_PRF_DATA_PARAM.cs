using System.Runtime.InteropServices;

using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI80.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Unicode)]
internal struct CK_PRF_DATA_PARAM
{
    public NativeULong type;
    public IntPtr pValue;
    public NativeULong ulValueLen;
}
