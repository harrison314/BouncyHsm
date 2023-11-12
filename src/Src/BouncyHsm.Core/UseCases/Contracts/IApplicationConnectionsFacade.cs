namespace BouncyHsm.Core.UseCases.Contracts;

public interface IApplicationConnectionsFacade
{
    ValueTask<DomainResult<SlotConnections>> GetApplicationConnections(CancellationToken cancellationToken);

    ValueTask<VoidDomainResult> RemoveApplicationConnection(Guid applicationSessionId, CancellationToken cancellationToken);
}
