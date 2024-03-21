namespace BouncyHsm.Core.UseCases.Contracts;

public interface IPkcsFacade
{
    ValueTask<DomainResult<Guid>> ImportP12(ImportP12Request request, CancellationToken cancellationToken);

    ValueTask<DomainResult<PkcsObjects>> GetObjects(uint slotId, CancellationToken cancellationToken);

    ValueTask<DomainResult<byte[]>> GeneratePkcs10(GeneratePkcs10Request request, CancellationToken cancellationToken);

    ValueTask<DomainResult<Guid>> GenerateSelfSignedCert(GenerateSelfSignedCertRequest request, CancellationToken cancellationToken);
    
    ValueTask<DomainResult<Guid>> ImportX509Certificate(ImportX509CertificateRequest request, CancellationToken cancellationToken);

    ValueTask<VoidDomainResult> DeleteAssociatedObjects(uint slotId, Guid objectId, CancellationToken cancellationToken);

    ValueTask<DomainResult<CertificateDetail>> ParseCertificate(uint slotId, Guid objectId, CancellationToken cancellationToken);

    ValueTask<DomainResult<IReadOnlyList<Guid>>> ImportPem(ImportPemRequest request, CancellationToken cancellationToken);
}
