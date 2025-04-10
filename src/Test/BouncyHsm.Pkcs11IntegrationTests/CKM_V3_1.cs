using Net.Pkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

internal static class CKM_V3_1
{
    public const CKM CKM_SHA_1_KEY_GEN = (CKM)0x00004003;
    public const CKM CKM_SHA224_KEY_GEN = (CKM)0x00004004;
    public const CKM CKM_SHA256_KEY_GEN = (CKM)0x00004005;
    public const CKM CKM_SHA384_KEY_GEN = (CKM)0x00004006;
    public const CKM CKM_SHA512_KEY_GEN = (CKM)0x00004007;
    public const CKM CKM_SHA512_224_KEY_GEN = (CKM)0x00004008;
    public const CKM CKM_SHA512_256_KEY_GEN = (CKM)0x00004009;
    public const CKM CKM_SHA512_T_KEY_GEN = (CKM)0x0000400a;

    public const CKM CKM_SHA3_256 = (CKM)0x000002b0;
    public const CKM CKM_SHA3_256_HMAC = (CKM)0x000002b1;
    public const CKM CKM_SHA3_256_HMAC_GENERAL = (CKM)0x000002b2;
    public const CKM CKM_SHA3_256_KEY_GEN = (CKM)0x000002b3;
    public const CKM CKM_SHA3_224 = (CKM)0x000002b5;
    public const CKM CKM_SHA3_224_HMAC = (CKM)0x000002b6;
    public const CKM CKM_SHA3_224_HMAC_GENERAL = (CKM)0x000002b7;
    public const CKM CKM_SHA3_224_KEY_GEN = (CKM)0x000002b8;
    public const CKM CKM_SHA3_384 = (CKM)0x000002c0;
    public const CKM CKM_SHA3_384_HMAC = (CKM)0x000002c1;
    public const CKM CKM_SHA3_384_HMAC_GENERAL = (CKM)0x000002c2;
    public const CKM CKM_SHA3_384_KEY_GEN = (CKM)0x000002c3;
    public const CKM CKM_SHA3_512 = (CKM)0x000002d0;
    public const CKM CKM_SHA3_512_HMAC = (CKM)0x000002d1;
    public const CKM CKM_SHA3_512_HMAC_GENERAL = (CKM)0x000002d2;
    public const CKM CKM_SHA3_512_KEY_GEN = (CKM)0x000002d3;

    public const CKM CKM_SHA3_256_KEY_DERIVATION = (CKM)0x00000397;
    public const CKM CKM_SHA3_224_KEY_DERIVATION = (CKM)0x00000398;
    public const CKM CKM_SHA3_384_KEY_DERIVATION = (CKM)0x00000399;
    public const CKM CKM_SHA3_512_KEY_DERIVATION = (CKM)0x0000039a;

    public const CKM CKM_SHA3_256_RSA_PKCS = (CKM)0x00000060;
    public const CKM CKM_SHA3_384_RSA_PKCS = (CKM)0x00000061;
    public const CKM CKM_SHA3_512_RSA_PKCS = (CKM)0x00000062;
    public const CKM CKM_SHA3_256_RSA_PKCS_PSS = (CKM)0x00000063;
    public const CKM CKM_SHA3_384_RSA_PKCS_PSS = (CKM)0x00000064;
    public const CKM CKM_SHA3_512_RSA_PKCS_PSS = (CKM)0x00000065;
    public const CKM CKM_SHA3_224_RSA_PKCS = (CKM)0x00000066;
    public const CKM CKM_SHA3_224_RSA_PKCS_PSS = (CKM)0x00000067;
}
