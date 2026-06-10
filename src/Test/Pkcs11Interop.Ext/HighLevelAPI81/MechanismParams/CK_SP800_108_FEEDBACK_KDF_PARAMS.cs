using System.Runtime.InteropServices;

using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI81.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
internal struct CK_SP800_108_FEEDBACK_KDF_PARAMS
{
    public NativeULong prfType;
    public NativeULong ulNumberOfDataParams;
    public IntPtr pDataParams; //CK_PRF_DATA_PARAM
    public NativeULong ulIVLen;
    public IntPtr pIV;
    public NativeULong ulAdditionalDerivedKeys;
    public IntPtr pAdditionalDerivedKeys;
}
