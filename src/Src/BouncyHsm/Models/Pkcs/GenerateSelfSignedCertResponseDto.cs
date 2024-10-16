namespace BouncyHsm.Models.Pkcs;

public class GenerateSelfSignedCertResponseDto
{
    public Guid CertificateId
    {
        get;
        set;
    }

    public GenerateSelfSignedCertResponseDto()
    {
        
    }

    internal GenerateSelfSignedCertResponseDto(Guid certificateId)
    {
        this.CertificateId = certificateId;
    }
}