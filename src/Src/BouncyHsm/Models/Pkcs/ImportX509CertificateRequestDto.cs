namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.ImportX509CertificateRequest), IgnoredMembers = new string[] { "SlotId" })]
public class ImportX509CertificateRequestDto
{
    public Guid PrivateKeyId
    {
        get;
        set;
    }

    public byte[] Certificate
    {
        get;
        set;
    }

    public ImportX509CertificateRequestDto()
    {
        this.Certificate = Array.Empty<byte>();
    }
}