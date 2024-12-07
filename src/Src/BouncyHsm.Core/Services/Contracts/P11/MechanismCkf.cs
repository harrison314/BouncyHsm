namespace BouncyHsm.Core.Services.Contracts.P11;

[Flags]
public enum MechanismCkf : uint
{
    NONE = 0x00000000,
    CKF_HW = 0x00000001,
    CKF_ENCRYPT = 0x00000100,
    CKF_DECRYPT = 0x00000200,
    CKF_DIGEST = 0x00000400,
    CKF_SIGN = 0x00000800,
    CKF_SIGN_RECOVER = 0x00001000,
    CKF_VERIFY = 0x00002000,
    CKF_VERIFY_RECOVER = 0x00004000,
    CKF_GENERATE = 0x00008000,
    CKF_GENERATE_KEY_PAIR = 0x00010000,
    CKF_WRAP = 0x00020000,
    CKF_UNWRAP = 0x00040000,
    CKF_DERIVE = 0x00080000,

    /// <summary>
    /// True if the mechanism can be used with EC domain parameters over F_p
    /// </summary>
    CKF_EC_F_P = 0x00100000,

    /// <summary>
    /// True if the mechanism can be used with EC domain parameters over F 2 m 
    /// </summary>
    CKF_EC_F_2M = 0x00200000,

    /// <summary>
    /// True if the mechanism can be used with EC domain parameters of the choice ecParameters
    /// </summary>
    CKF_EC_ECPARAMETERS = 0x00400000,

    /// <summary>
    /// True if the mechanism can be used with EC domain parameters of the choice namedCurve
    /// </summary>
    CKF_EC_NAMEDCURVE = 0x00800000,

    /// <summary>
    /// True if the mechanism can be used with elliptic curve point uncompressed 
    /// </summary>
    CKF_EC_UNCOMPRESS = 0x01000000,

    /// <summary>
    /// True if the mechanism can be used with elliptic curve point compressed 
    /// </summary>
    CKF_EC_COMPRESS = 0x02000000,
}