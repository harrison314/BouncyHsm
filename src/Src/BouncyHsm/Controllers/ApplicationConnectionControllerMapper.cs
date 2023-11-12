using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.ApplicationConnection;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class ApplicationConnectionControllerMapper
{
    public static partial List<ApplicationSessionDto> ToDto(List<ApplicationSession> list);
}