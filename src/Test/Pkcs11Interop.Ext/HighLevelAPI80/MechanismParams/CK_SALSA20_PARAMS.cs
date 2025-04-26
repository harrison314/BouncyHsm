using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI80.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 0, CharSet = CharSet.Unicode)]
internal struct CK_SALSA20_PARAMS
{
    public IntPtr pBlockCounter;
    public IntPtr pNonce;
    public NativeULong ulNonceBits;
}
