namespace BouncyHsm.Core.Services.P11Handlers.Common;

public enum Pkcs11SpecVersion
{
    /// <summary>
    /// PKCS#11 v2.40 https://docs.oasis-open.org/pkcs11/pkcs11-curr/v2.40/os/pkcs11-curr-v2.40-os.pdf
    /// </summary>
    V2_40,

    /// <summary>
    /// PKCS#11 v3.0 https://docs.oasis-open.org/pkcs11/pkcs11-curr/v3.0/pkcs11-curr-v3.0.pdf
    /// </summary>
    V3_0
}
