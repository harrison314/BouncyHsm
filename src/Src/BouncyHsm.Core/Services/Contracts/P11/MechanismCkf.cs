namespace BouncyHsm.Core.Services.Contracts.P11;

[Flags]
public enum MechanismCkf : uint
{
    /// <summary>
    /// Without CKF
    /// </summary>
    NONE = 0x00000000,

    /// <summary>
    /// True if the mechanism is performed by the device; false if the mechanism is performed in software 
    /// </summary>
    CKF_HW = 0x00000001,

    /// <summary>
    /// True if the mechanism can be used with C_EncryptInit
    /// </summary>
    CKF_ENCRYPT = 0x00000100,

    /// <summary>
    /// True if the mechanism can be used with C_DecryptInit
    /// </summary>
    CKF_DECRYPT = 0x00000200,

    /// <summary>
    /// True if the mechanism can be used with C_DigestInit
    /// </summary>
    CKF_DIGEST = 0x00000400,

    /// <summary>
    /// True if the mechanism can be used with C_SignInit
    /// </summary>
    CKF_SIGN = 0x00000800,

    /// <summary>
    /// True if the mechanism can be used with C_SignRecoverInit
    /// </summary>
    CKF_SIGN_RECOVER = 0x00001000,

    /// <summary>
    /// True if the mechanism can be used with C_VerifyInit
    /// </summary>
    CKF_VERIFY = 0x00002000,

    /// <summary>
    /// True if the mechanism can be used with C_VerifyRecoverInit
    /// </summary>
    CKF_VERIFY_RECOVER = 0x00004000,

    /// <summary>
    /// True if the mechanism can be used with C_GenerateKey
    /// </summary>
    CKF_GENERATE = 0x00008000,

    /// <summary>
    /// True if the mechanism can be used with C_GenerateKeyPair
    /// </summary>
    CKF_GENERATE_KEY_PAIR = 0x00010000,

    /// <summary>
    /// True if the mechanism can be used with C_WrapKey
    /// </summary>
    CKF_WRAP = 0x00020000,

    /// <summary>
    /// True if the mechanism can be used with C_UnwrapKey
    /// </summary>
    CKF_UNWRAP = 0x00040000,

    /// <summary>
    /// True if the mechanism can be used with C_DeriveKey
    /// </summary>
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

    /// <summary>
    /// True if there is an extension to the flags; false if no extensions. Must be false for this version. 
    /// </summary>
    CKF_EXTENSION = 0x80000000,

    /// <summary>
    /// True if the mechanism can be used with EC domain parameters of the choice curveName (ASN1 printed string)
    /// </summary>
    CKF_EC_CURVENAME = 0x04000000
}