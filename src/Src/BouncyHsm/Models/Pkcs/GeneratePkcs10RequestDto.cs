namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.GeneratePkcs10Request), IgnoredMembers = new string[] { "SlotId" })]
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
