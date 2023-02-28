namespace BouncyHsm.Core.UseCases.Contracts;

public interface IPkcsFacade
{
    ValueTask<DomainResult<Guid>> ImportP12(ImportP12Request request, CancellationToken cancellationToken);

    ValueTask<DomainResult<PkcsObjects>> GetObjects(uint slotId, CancellationToken cancellationToken);
}
