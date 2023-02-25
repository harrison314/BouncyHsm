/*
 * This code is from https://github.com/Pkcs11Interop/Pkcs11Interop
 * Copyright The Pkcs11Interop Project under Apache License Version 2.0.
 * Written for the Pkcs11Interop project by: Jaroslav IMRICH <jimrich@jimrich.sk>
 */

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// Types of Cryptoki users
/// </summary>
public enum CKU : uint
{
    /// <summary>
    /// Security Officer
    /// </summary>
    CKU_SO = 0,

    /// <summary>
    /// Normal user
    /// </summary>
    CKU_USER = 1,

    /// <summary>
    /// Context specific
    /// </summary>
    CKU_CONTEXT_SPECIFIC = 2
}
