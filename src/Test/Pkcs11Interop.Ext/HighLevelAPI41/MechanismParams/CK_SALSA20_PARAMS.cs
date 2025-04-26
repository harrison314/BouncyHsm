using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NativeULong = System.UInt32;

namespace Pkcs11Interop.Ext.HighLevelAPI41.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
internal struct CK_SALSA20_PARAMS
{
    public IntPtr pBlockCounter;
    public IntPtr pNonce;
    public NativeULong ulNonceBits;
}
