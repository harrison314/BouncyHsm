﻿using BouncyHsm.Core.UseCases.Contracts;
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

    public static partial GenerateAesKeyRequest MapFromDto(GenerateAesKeyRequestDto dto);

    public static partial GenerateSecretKeyRequest MapFromDto(GenerateSecretKeyRequestDto dto);

}
