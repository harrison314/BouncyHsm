using System.Runtime.InteropServices;

using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI80.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Unicode)]
internal struct CK_HKDF_PARAMS
{
    public byte bExtract;
    public byte bExpand;
    public NativeULong prfHashMechanism;
    public NativeULong ulSaltType;
    public IntPtr pSalt;
    public NativeULong ulSaltLen;
    public NativeULong hSaltKey;
    public IntPtr pInfo;
    public NativeULong ulInfoLen;
}