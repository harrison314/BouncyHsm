using Net.Pkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

internal static class CKM_V3_1
{
    public const CKM CKM_SHA3_256 = (CKM) 0x000002b0;
    public const CKM CKM_SHA3_256_HMAC = (CKM) 0x000002b1;
    public const CKM CKM_SHA3_256_HMAC_GENERAL = (CKM) 0x000002b2;
    public const CKM CKM_SHA3_256_KEY_GEN = (CKM) 0x000002b3;
    public const CKM CKM_SHA3_224 = (CKM) 0x000002b5;
    public const CKM CKM_SHA3_224_HMAC = (CKM) 0x000002b6;
    public const CKM CKM_SHA3_224_HMAC_GENERAL = (CKM) 0x000002b7;
    public const CKM CKM_SHA3_224_KEY_GEN = (CKM) 0x000002b8;
    public const CKM CKM_SHA3_384 = (CKM) 0x000002c0;
    public const CKM CKM_SHA3_384_HMAC = (CKM) 0x000002c1;
    public const CKM CKM_SHA3_384_HMAC_GENERAL = (CKM) 0x000002c2;
    public const CKM CKM_SHA3_384_KEY_GEN = (CKM) 0x000002c3;
    public const CKM CKM_SHA3_512 = (CKM) 0x000002d0;
    public const CKM CKM_SHA3_512_HMAC = (CKM) 0x000002d1;
    public const CKM CKM_SHA3_512_HMAC_GENERAL = (CKM) 0x000002d2;
    public const CKM CKM_SHA3_512_KEY_GEN = (CKM) 0x000002d3;
}
