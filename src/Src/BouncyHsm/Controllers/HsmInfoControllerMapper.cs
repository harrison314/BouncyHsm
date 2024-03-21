using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.HsmInfo;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class HsmInfoControllerMapper
{
    public static partial EcCurveInfoDto ToDto(SupportedNameCurve entity);

    public static partial IEnumerable<EcCurveInfoDto> ToDto(IEnumerable<SupportedNameCurve> entitys);

    public static partial BouncyHsmVersionDto ToDto(BouncyHsmVersion bouncyHsmVersion);

    public static partial MechanismInfoDto ToDto(MechanismInfoData mechanismInfoData);

    public static partial IEnumerable<MechanismInfoDto> ToDto(IEnumerable<MechanismInfoData> mechanismInfoData);

    public static partial MechanismProfileDto ToDto(MechanismProfile mechanismProfile);

    private static MechanismFlags MapFlags(MechanismCkf flags)
    {
        return new MechanismFlags()
        {
            Decrypt = flags.HasFlag(MechanismCkf.CKF_DECRYPT),
            Derive = flags.HasFlag(MechanismCkf.CKF_DERIVE),
            Digest = flags.HasFlag(MechanismCkf.CKF_DIGEST),
            Generate = flags.HasFlag(MechanismCkf.CKF_GENERATE),
            GenerateKeyPair = flags.HasFlag(MechanismCkf.CKF_GENERATE_KEY_PAIR),
            Encrypt = flags.HasFlag(MechanismCkf.CKF_ENCRYPT),
            Sign = flags.HasFlag(MechanismCkf.CKF_SIGN),
            SignRecover = flags.HasFlag(MechanismCkf.CKF_SIGN_RECOVER),
            Unwrap = flags.HasFlag(MechanismCkf.CKF_UNWRAP),
            Verify = flags.HasFlag(MechanismCkf.CKF_VERIFY),
            VerifyRecover = flags.HasFlag(MechanismCkf.CKF_VERIFY_RECOVER),
            Wrap = flags.HasFlag(MechanismCkf.CKF_WRAP),
        };
    }

    private static string MapMechanism(CKM mechanism)
    {
        // Disabling use generating Mapperly fast to string
        return mechanism.ToString();
    }
}