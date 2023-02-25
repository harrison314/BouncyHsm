using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.StorageObjects;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Controllers;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal static partial class StorageObjectsControllerMapper
{
    public static partial StorageObjectsListDto ToDto(StorageObjectsList storageObjectList);

    public static partial StorageObjectDetailDto ToDto(StorageObjectDetail storageObjectDetail);

    private static string MapAttributeType(CKA attributeType)
    {
        // Disabling use generating Mapperly fast to string
        return attributeType.ToString();
    }
}