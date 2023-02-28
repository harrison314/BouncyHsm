namespace BouncyHsm.Core.UseCases.Contracts;

public interface IPkcsFacade
{
    ValueTask<DomainResult<Guid>> ImportP12(ImportP12Request request, CancellationToken cancellationToken);

    ValueTask<DomainResult<PkcsObjects>> GetObjects(uint slotId, CancellationToken cancellationToken);

    ValueTask<DomainResult<byte[]>> GeneratePkcs10(GeneratePkcs10Request request, CancellationToken cancellationToken);

}

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

[Dunet.Union]
public partial record SubjectName
{
    partial record Text(string X509NameText);
    partial record OidValuePairs(List<SubjectNameEntry> Pairs);
}

public record SubjectNameEntry(string Oid, string Value);