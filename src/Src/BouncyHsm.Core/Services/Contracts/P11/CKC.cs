/*
 * This code is from https://github.com/Pkcs11Interop/Pkcs11Interop
 * Copyright The Pkcs11Interop Project under Apache License Version 2.0.
 * Written for the Pkcs11Interop project by: Jaroslav IMRICH <jimrich@jimrich.sk>
 */

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// Certificate types
/// </summary>
public enum CKC : uint
{
    /// <summary>
    /// X.509 public key certificate
    /// </summary>
    CKC_X_509 = 0x00000000,

    /// <summary>
    /// X.509 attribute certificate
    /// </summary>
    CKC_X_509_ATTR_CERT = 0x00000001,

    /// <summary>
    /// WTLS public key certificate
    /// </summary>
    CKC_WTLS = 0x00000002,

    /// <summary>
    /// Permanently reserved for token vendors
    /// </summary>
    CKC_VENDOR_DEFINED = 0x80000000
}