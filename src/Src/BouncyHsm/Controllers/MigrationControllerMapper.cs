using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Migration;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
EnumMappingIgnoreCase = false,
ThrowOnMappingNullMismatch = true,
ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class MigrationControllerMapper
{
    public static partial MigrationResultDto ToDto(MigrationResult model);
}
