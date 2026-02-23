using System.Runtime.InteropServices;
using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI80.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
internal struct CK_SIGN_ADDITIONAL_CONTEXT
{
    public NativeULong hedgeVariant;
    public IntPtr pContext;
    public NativeULong ulContextLen;
}