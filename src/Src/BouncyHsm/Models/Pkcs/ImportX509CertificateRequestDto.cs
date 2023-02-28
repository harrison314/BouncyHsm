namespace BouncyHsm.Models.Pkcs;

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