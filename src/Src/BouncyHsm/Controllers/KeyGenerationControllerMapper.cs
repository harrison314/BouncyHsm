using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.KeyGeneration;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
EnumMappingIgnoreCase = false,
ThrowOnMappingNullMismatch = true,
ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class KeyGenerationControllerMapper
{
    public static partial GeneratedKeyPairIdsDto ToDto(GeneratedKeyPairIds dto);
    public static partial GeneratedSecretIdDto ToDto(GeneratedSecretId dto);

    private static partial GenerateKeyAttributes MapFromDto(GenerateKeyAttributesDto dto);

    public static partial GenerateRsaKeyPairRequest MapFromDto(GenerateRsaKeyPairRequestDto dto);

    public static partial GenerateEcKeyPairRequest MapFromDto(GenerateEcKeyPairRequestDto dto);

    public static partial GenerateEdwardsKeyPairRequest MapFromDto(GenerateEdwardsKeyPairRequestDto dto);

    public static partial GenerateAesKeyRequest MapFromDto(GenerateAesKeyRequestDto dto);

    public static partial GenerateSecretKeyRequest MapFromDto(GenerateSecretKeyRequestDto dto);

    public static partial GeneratePoly1305KeyRequest MapFromDto(GeneratePoly1305KeyRequestDto dto);

    public static partial GenerateChaCha20KeyRequest MapFromDto(GenerateChaCha20KeyRequestDto dto);

    public static partial GenerateSalsa20KeyRequest MapFromDto(GenerateSalsa20KeyRequestDto dto);

    public static partial GenerateMontgomeryKeyPairRequest MapFromDto(GenerateMontgomeryKeyPairRequestDto model);

    public static partial GenerateMLDsaKeyPairRequest MapFromDto(GenerateMLDsaKeyPairRequestDto model);

    public static partial GenerateSlhDsaKeyPairRequest MapFromDto(GenerateSlhDsaKeyPairRequestDto model);

    /// <summary>
    /// Maps HSS request. LMS and LM-OTS types are transferred as names; unknown names throw <see cref="ArgumentException"/>.
    /// </summary>
    public static GenerateHssKeyPairRequest MapFromDto(GenerateHssKeyPairRequestDto model)
    {
        return new GenerateHssKeyPairRequest()
        {
            Levels = model.Levels,
            LmsTypes = ParseEnumNames<CK_LMS_TYPE>(model.LmsTypes, nameof(model.LmsTypes)),
            LmotsTypes = ParseEnumNames<CK_LMOTS_TYPE>(model.LmotsTypes, nameof(model.LmotsTypes)),
            KeyAttributes = MapFromDto(model.KeyAttributes),
        };
    }

    private static T[] ParseEnumNames<T>(string[] names, string propertyName)
        where T : struct, Enum
    {
        T[] result = new T[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            if (!Enum.TryParse(names[i], false, out T value) || !Enum.IsDefined(value))
            {
                throw new ArgumentException($"Value '{names[i]}' in {propertyName} is not valid.");
            }

            result[i] = value;
        }

        return result;
    }

    public static partial GenerateMLKemKeyPairRequest MapFromDto(GenerateMlKemKeyPairRequestDto model);

    public static partial GenerateCamelliaKeyRequest MapFromDto(GenerateCamelliaKeyRequestDto model);
}
