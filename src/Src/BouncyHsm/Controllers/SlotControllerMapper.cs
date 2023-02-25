using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Slot;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class SlotControllerMapper
{
    public static partial CreateSlotData MapFromDto(CreateSlotDto dto);

    public static partial SlotDto ToDto(SlotEntity entity);

    public static partial List<SlotDto> ToDto(IReadOnlyList<SlotEntity> entities);

    public static partial CreateSlotResultDto ToDto(CreateSlotResult result);
}
