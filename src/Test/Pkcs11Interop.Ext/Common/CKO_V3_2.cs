using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net.Pkcs11Interop.Common;

namespace Pkcs11Interop.Ext.Common;

public static class CKO_V3_2
{
    public const CKO CKO_PROFILE = (CKO) 0x00000009U;
    public const CKO CKO_VALIDATION = (CKO)0x0000000aU;
    public const CKO CKO_TRUST = (CKO)0x0000000bU;
}
