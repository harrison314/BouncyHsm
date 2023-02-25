/*
 * This code is from https://github.com/Pkcs11Interop/Pkcs11Interop
 * Copyright The Pkcs11Interop Project under Apache License Version 2.0.
 * Written for the Pkcs11Interop project by: Jaroslav IMRICH <jimrich@jimrich.sk>
 */

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// Mask generation functions
/// </summary>
public enum CKG : uint
{
    /// <summary>
    /// PKCS #1 Mask Generation Function with SHA-1 digest algorithm
    /// </summary>
    CKG_MGF1_SHA1 = 0x00000001,

    /// <summary>
    /// PKCS #1 Mask Generation Function with SHA-256 digest algorithm
    /// </summary>
    CKG_MGF1_SHA256 = 0x00000002,

    /// <summary>
    /// PKCS #1 Mask Generation Function with SHA-384 digest algorithm
    /// </summary>
    CKG_MGF1_SHA384 = 0x00000003,

    /// <summary>
    /// PKCS #1 Mask Generation Function with SHA-512 digest algorithm
    /// </summary>
    CKG_MGF1_SHA512 = 0x00000004,

    /// <summary>
    /// PKCS #1 Mask Generation Function with SHA-224 digest algorithm
    /// </summary>
    CKG_MGF1_SHA224 = 0x00000005
}