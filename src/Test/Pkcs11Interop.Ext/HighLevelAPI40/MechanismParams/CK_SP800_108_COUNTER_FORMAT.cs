using System.Runtime.InteropServices;
using NativeULong = System.UInt32;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Unicode)]
internal struct CK_SP800_108_COUNTER_FORMAT
{
    public byte bLittleEndian;
    public NativeULong ulWidthInBits;
}

