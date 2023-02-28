using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Models.Pkcs;

public record PkcsSpecificObjectDto(CKO CkaClass, Guid ObjectId, string Description);

public class GeneratePkcs10RequestDto
{
    public Guid PrivateKeyId
    {
        get;
        set;
    }

    public Guid PublicKeyId
    {
        get;
        set;
    }

    public SubjectNameDto Subject
    {
        get;
        set;
    }

    public GeneratePkcs10RequestDto()
    {
        this.Subject = new SubjectNameDto();
    }
}
