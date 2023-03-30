using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Stats;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class StatsControllerMapper
{
    public static partial OverviewStatsDto ToDto(OverviewStats stats);
}