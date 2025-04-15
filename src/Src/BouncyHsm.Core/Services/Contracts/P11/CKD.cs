/*
 * This code is from https://github.com/Pkcs11Interop/Pkcs11Interop
 * Copyright The Pkcs11Interop Project under Apache License Version 2.0.
 * Written for the Pkcs11Interop project by: Jaroslav IMRICH <jimrich@jimrich.sk>
 */

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// Key derivation functions
/// </summary>
public enum CKD : uint
{
    /// <summary>
    /// No derivation function
    /// </summary>
    CKD_NULL = 0x00000001,

    /// <summary>
    /// ANSI X9.63 key derivation function based on SHA-1
    /// </summary>
    CKD_SHA1_KDF = 0x00000002,

    /// <summary>
    /// ANSI X9.42 key derivation function based on SHA-1
    /// </summary>
    CKD_SHA1_KDF_ASN1 = 0x00000003,

    /// <summary>
    /// ANSI X9.42 key derivation function based on SHA-1
    /// </summary>
    CKD_SHA1_KDF_CONCATENATE = 0x00000004,

    /// <summary>
    /// ANSI X9.63 key derivation function based on SHA-224
    /// </summary>
    CKD_SHA224_KDF = 0x00000005,

    /// <summary>
    /// ANSI X9.63 key derivation function based on SHA-256
    /// </summary>
    CKD_SHA256_KDF = 0x00000006,

    /// <summary>
    /// ANSI X9.63 key derivation function based on SHA-384
    /// </summary>
    CKD_SHA384_KDF = 0x00000007,

    /// <summary>
    /// ANSI X9.63 key derivation function based on SHA-512
    /// </summary>
    CKD_SHA512_KDF = 0x00000008,

    /// <summary>
    /// CryptoPro KEK Diversification Algorithm described in section 6.5 of RFC 4357 
    /// </summary>
    CKD_CPDIVERSIFY_KDF = 0x00000009,

    // Specification 3.0

    CKD_SHA3_224_KDF = 0x0000000A,
    CKD_SHA3_256_KDF = 0x0000000B,
    CKD_SHA3_384_KDF = 0x0000000C,
    CKD_SHA3_512_KDF = 0x0000000D,
    CKD_BLAKE2B_160_KDF = 0x00000017,
    CKD_BLAKE2B_256_KDF = 0x00000018,
    CKD_BLAKE2B_384_KDF = 0x00000019,
    CKD_BLAKE2B_512_KDF = 0x0000001a,
}