namespace BouncyHsm.Core.UseCases.Contracts;

public class ImportX509CertificateRequest
{
    public uint SlotId
    {
        get;
        set;
    }

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

    public ImportX509CertificateRequest()
    {
        this.Certificate = Array.Empty<byte>();
    }
}