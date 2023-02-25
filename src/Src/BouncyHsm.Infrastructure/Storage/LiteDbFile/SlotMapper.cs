using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;
using Riok.Mapperly.Abstractions;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName, 
    EnumMappingIgnoreCase = false,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true)]
internal partial class SlotMapper
{
    public SlotMapper()
    {

    }

    public partial SlotModel MapSlot(SlotEntity slotEntity);

    public partial SlotEntity MapSlot(SlotModel model);
}