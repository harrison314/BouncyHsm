using Net.Pkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.Common;

public static class CKM_V3_0
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

    public const CKM CKM_ECDSA_SHA3_224 = (CKM)0x00001047;
    public const CKM CKM_ECDSA_SHA3_256 = (CKM)0x00001048;
    public const CKM CKM_ECDSA_SHA3_384 = (CKM)0x00001049;
    public const CKM CKM_ECDSA_SHA3_512 = (CKM)0x0000104a;

    public const CKM CKM_BLAKE2B_160 = (CKM)0x0000400c;
    public const CKM CKM_BLAKE2B_160_HMAC = (CKM)0x0000400d;
    public const CKM CKM_BLAKE2B_160_HMAC_GENERAL = (CKM)0x0000400e;
    public const CKM CKM_BLAKE2B_160_KEY_DERIVE = (CKM)0x0000400f;
    public const CKM CKM_BLAKE2B_160_KEY_GEN = (CKM)0x00004010;
    public const CKM CKM_BLAKE2B_256 = (CKM)0x00004011;
    public const CKM CKM_BLAKE2B_256_HMAC = (CKM)0x00004012;
    public const CKM CKM_BLAKE2B_256_HMAC_GENERAL = (CKM)0x00004013;
    public const CKM CKM_BLAKE2B_256_KEY_DERIVE = (CKM)0x00004014;
    public const CKM CKM_BLAKE2B_256_KEY_GEN = (CKM)0x00004015;
    public const CKM CKM_BLAKE2B_384 = (CKM)0x00004016;
    public const CKM CKM_BLAKE2B_384_HMAC = (CKM)0x00004017;
    public const CKM CKM_BLAKE2B_384_HMAC_GENERAL = (CKM)0x00004018;
    public const CKM CKM_BLAKE2B_384_KEY_DERIVE = (CKM)0x00004019;
    public const CKM CKM_BLAKE2B_384_KEY_GEN = (CKM)0x0000401a;
    public const CKM CKM_BLAKE2B_512 = (CKM)0x0000401b;
    public const CKM CKM_BLAKE2B_512_HMAC = (CKM)0x0000401c;
    public const CKM CKM_BLAKE2B_512_HMAC_GENERAL = (CKM)0x0000401d;
    public const CKM CKM_BLAKE2B_512_KEY_DERIVE = (CKM)0x0000401e;
    public const CKM CKM_BLAKE2B_512_KEY_GEN = (CKM)0x0000401f;

    public const CKM CKM_POLY1305_KEY_GEN = (CKM)0x00001227;
    public const CKM CKM_POLY1305 = (CKM)0x00001228;
}
