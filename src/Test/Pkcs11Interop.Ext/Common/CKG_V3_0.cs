using Net.Pkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.Common;

public static class CKG_V3_0
{
    public const CKG CKG_MGF1_SHA3_224 = (CKG)0x00000006;
    public const CKG CKG_MGF1_SHA3_256 = (CKG)0x00000007;
    public const CKG CKG_MGF1_SHA3_384 = (CKG)0x00000008;
    public const CKG CKG_MGF1_SHA3_512 = (CKG)0x00000009;
}
