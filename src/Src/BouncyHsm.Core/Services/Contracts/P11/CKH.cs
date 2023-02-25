/*
 * This code is from https://github.com/Pkcs11Interop/Pkcs11Interop
 * Copyright The Pkcs11Interop Project under Apache License Version 2.0.
 * Written for the Pkcs11Interop project by: Jaroslav IMRICH <jimrich@jimrich.sk>
 */

namespace BouncyHsm.Core.Services.Contracts.P11;

/// <summary>
/// Hardware feature types
/// </summary>
public enum CKH : uint
{
    /// <summary>
    /// Monotonic counter objects represent hardware counters that exist on the device.
    /// </summary>
    CKH_MONOTONIC_COUNTER = 0x00000001,

    /// <summary>
    /// Clock objects represent real-time clocks that exist on the device.
    /// </summary>
    CKH_CLOCK = 0x00000002,

    /// <summary>
    /// User interface objects represent the presentation capabilities of the device.
    /// </summary>
    CKH_USER_INTERFACE = 0x00000003,

    /// <summary>
    /// Permanently reserved for token vendors.
    /// </summary>
    CKH_VENDOR_DEFINED = 0x80000000
}