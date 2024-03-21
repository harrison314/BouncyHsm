namespace BouncyHsm.Core.UseCases.Contracts;

public class GenerateSelfSignedCertRequest
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

    public TimeSpan Validity
    {
        get;
        set;
    }

    public GenerateSelfSignedCertRequest()
    {
        this.Subject = default!;
    }
}