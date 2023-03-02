namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.CertificateDetail))]
public class CertificateDetailDto
{
    public string Subject
    {
        get;
        set;
    }

    public string Issuer
    {
        get;
        set;
    }

    public DateTime NotAfter
    {
        get;
        set;
    }

    public DateTime NotBefore
    {
        get;
        set;
    }

    public string SerialNumber
    {
        get;
        set;
    }

    public string Thumbprint
    {
        get;
        set;
    }

    public string SignatureAlgorithm
    {
        get;
        set;
    }

    public CertificateDetailDto()
    {
        this.Subject = string.Empty;
        this.Issuer = string.Empty;
        this.SerialNumber = string.Empty;
        this.Thumbprint = string.Empty;
        this.SignatureAlgorithm = string.Empty;
    }
}