using Net.Pkcs11Interop.Common;

namespace Pkcs11Interop.Ext.Common;

public static class CKD_V3_0
{
    public const CKD CKD_SHA3_224_KDF = (CKD)0x0000000A;
    public const CKD CKD_SHA3_256_KDF = (CKD)0x0000000B;
    public const CKD CKD_SHA3_384_KDF = (CKD)0x0000000C;
    public const CKD CKD_SHA3_512_KDF = (CKD)0x0000000D;
    public const CKD CKD_BLAKE2B_160_KDF = (CKD)0x00000017;
    public const CKD CKD_BLAKE2B_256_KDF = (CKD)0x00000018;
    public const CKD CKD_BLAKE2B_384_KDF = (CKD)0x00000019;
    public const CKD CKD_BLAKE2B_512_KDF = (CKD)0x0000001a;
}
