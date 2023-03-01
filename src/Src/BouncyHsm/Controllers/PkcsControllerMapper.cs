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

    public static GeneratePkcs10Request FromDto(GeneratePkcs10RequestDto dto, uint slotId)
    {
        GeneratePkcs10Request request = FromDto(dto);
        request.SlotId = slotId;

        return request;
    }

    [MapperIgnoreTarget(nameof(ImportP12Request.SlotId))]
    private static partial GeneratePkcs10Request FromDto(GeneratePkcs10RequestDto dto);

    private static SubjectName FromDto(SubjectNameDto dto)
    {
        if (dto.OidValuePairs != null)
        {
            return new SubjectName.OidValuePairs(FromSubjectNameDtos(dto.OidValuePairs));
        }

        if (dto.DirName != null)
        {
            return new SubjectName.Text(dto.DirName);
        }

        throw new InvalidDataException("");
    }

    private static partial List<SubjectNameEntry> FromSubjectNameDtos(List<SubjectNameEntryDto> subjectNames);

    public static ImportX509CertificateRequest FromDto(ImportX509CertificateRequestDto dto, uint slotId)
    {
        ImportX509CertificateRequest request = FromDto(dto);
        request.SlotId = slotId;

        return request;
    }

    [MapperIgnoreTarget(nameof(ImportP12Request.SlotId))]
    private static partial ImportX509CertificateRequest FromDto(ImportX509CertificateRequestDto dto);

    public static partial CertificateDetailDto ToDto(CertificateDetail detail);
}