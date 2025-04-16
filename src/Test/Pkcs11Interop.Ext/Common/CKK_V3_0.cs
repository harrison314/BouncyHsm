using Net.Pkcs11Interop.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.Common;

public static class CKK_V3_0
{
    public const CKK CKK_CHACHA20 = (CKK)0x00000033;
    public const CKK CKK_POLY1305 = (CKK)0x00000034;
    public const CKK CKK_AES_XTS = (CKK)0x00000035;
    public const CKK CKK_SHA3_224_HMAC = (CKK)0x00000036;
    public const CKK CKK_SHA3_256_HMAC = (CKK)0x00000037;
    public const CKK CKK_SHA3_384_HMAC = (CKK)0x00000038;
    public const CKK CKK_SHA3_512_HMAC = (CKK)0x00000039;
    public const CKK CKK_BLAKE2B_160_HMAC = (CKK)0x0000003a;
    public const CKK CKK_BLAKE2B_256_HMAC = (CKK)0x0000003b;
    public const CKK CKK_BLAKE2B_384_HMAC = (CKK)0x0000003c;
    public const CKK CKK_BLAKE2B_512_HMAC = (CKK)0x0000003d;
    public const CKK CKK_SALSA20 = (CKK)0x0000003e;
    public const CKK CKK_X2RATCHET = (CKK)0x0000003f;
    public const CKK CKK_EC_EDWARDS = (CKK)0x00000040;
    public const CKK CKK_EC_MONTGOMERY = (CKK)0x00000041;
    public const CKK CKK_HKDF = (CKK)0x00000042;

    public const CKK CKK_SHA512_224_HMAC = (CKK)0x00000043;
    public const CKK CKK_SHA512_256_HMAC = (CKK)0x00000044;
    public const CKK CKK_SHA512_T_HMAC = (CKK)0x00000045;
    public const CKK CKK_HSS = (CKK)0x00000046;
}
