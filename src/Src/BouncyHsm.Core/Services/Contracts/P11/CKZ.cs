/*
 * This code is from https://github.com/Pkcs11Interop/Pkcs11Interop
 * Copyright The Pkcs11Interop Project under Apache License Version 2.0.
 * Written for the Pkcs11Interop project by: Jaroslav IMRICH <jimrich@jimrich.sk>
 */

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// Salt/Encoding parameter sources
/// </summary>
public enum CKZ : uint
{
    /// <summary>
    /// PKCS #1 RSA OAEP: Encoding parameter specified or PKCS #5 PBKDF2 Key Generation: Salt specified
    /// </summary>
    CKZ_DATA_SPECIFIED_OR_CKZ_SALT_SPECIFIED = 0x00000001,

    ///// <summary>
    ///// PKCS #1 RSA OAEP: Encoding parameter specified
    ///// </summary>
    //CKZ_DATA_SPECIFIED = 0x00000001,

    ///// <summary>
    ///// PKCS #5 PBKDF2 Key Generation: Salt specified
    ///// </summary>
    //CKZ_SALT_SPECIFIED = 0x00000001,
}