using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;
using Riok.Mapperly.Abstractions;
using System.Runtime.CompilerServices;

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

    [MapperIgnoreTarget(nameof(SlotModel.Created))]
    [MapProperty(nameof(SlotEntity.IsPlugged), nameof(SlotModel.IsPlugged), Use = nameof(IsPluggedMapperReverse))]
    public partial SlotModel MapSlot(SlotEntity slotEntity);


    [MapperIgnoreSource(nameof(SlotModel.Created))]
    [MapProperty(nameof(SlotModel.IsPlugged), nameof(SlotEntity.IsPlugged), Use = nameof(IsPluggedMapper))]
    public partial SlotEntity MapSlot(SlotModel model);

    [UserMapping(Default = false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPluggedMapper(bool? value)
    {
        return value ?? false;
    }

    [UserMapping(Default = false)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool? IsPluggedMapperReverse(bool value)
    {
        return value;
    }
}