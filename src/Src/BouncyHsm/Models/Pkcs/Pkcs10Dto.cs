namespace BouncyHsm.Models.Pkcs;

public class Pkcs10Dto
{
    public byte[] Content
    {
        get;
        set;
    }

    public Pkcs10Dto(byte[] content)
    {
        this.Content = content;
    }
}