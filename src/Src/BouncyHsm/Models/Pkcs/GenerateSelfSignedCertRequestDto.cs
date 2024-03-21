namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.GenerateSelfSignedCertRequest), IgnoredMembers = new string[] { "SlotId" })]
public class GenerateSelfSignedCertRequestDto
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

    public TimeSpan Validity
    {
        get;
        set;
    }

    public GenerateSelfSignedCertRequestDto()
    {
        this.Subject = new SubjectNameDto();
    }
}
