namespace BouncyHsm.Core.UseCases.Contracts;

public class GeneratePkcs10Request
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

    public Guid PublicKeyId
    {
        get;
        set;
    }

    public SubjectName Subject
    {
        get;
        set;
    }

    public GeneratePkcs10Request()
    {
        this.Subject = default!;
    }
}
