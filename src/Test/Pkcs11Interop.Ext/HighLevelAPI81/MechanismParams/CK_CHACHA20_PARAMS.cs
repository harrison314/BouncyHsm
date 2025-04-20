using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using NativeULong = System.UInt64;

namespace Pkcs11Interop.Ext.HighLevelAPI81.MechanismParams;

[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
internal struct CK_CHACHA20_PARAMS
{
    public IntPtr pBlockCounter;
    public NativeULong blockCounterBits;
    public IntPtr pNonce;
    public NativeULong ulNonceBits;
}
