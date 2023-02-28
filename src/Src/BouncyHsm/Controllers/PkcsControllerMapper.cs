using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Pkcs;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class PkcsControllerMapper
{
    public static ImportP12Request FromDto(ImportP12RequestDto dto, uint slotId)
    {
        ImportP12Request request = FromDto(dto);
        request.SlotId = slotId;

        return request;
    }

    [MapperIgnoreTarget(nameof(ImportP12Request.SlotId))]
    private static partial ImportP12Request FromDto(ImportP12RequestDto dto);

    public static partial PkcsObjectsDto ToDto(PkcsObjects pkcsObjects);

    private static partial List<PkcsObjectInfoDto> ToDto(IReadOnlyList<PkcsObjectInfo> objects);
}