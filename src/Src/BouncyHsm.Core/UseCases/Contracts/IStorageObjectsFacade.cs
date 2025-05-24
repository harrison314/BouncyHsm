namespace BouncyHsm.Core.UseCases.Contracts;

public interface IStorageObjectsFacade
{
    ValueTask<DomainResult<StorageObjectsList>> GetStorageObjects(uint slotId, int skip, int take, CancellationToken cancellationToken);

    ValueTask<DomainResult<StorageObjectDetail>> GetStorageObject(uint slotId, Guid id, CancellationToken cancellationToken);

    ValueTask<VoidDomainResult> DeleteStorageObject(uint slotId, Guid id, CancellationToken cancellationToken);

    ValueTask<DomainResult<ObjectContent>> Download(uint slotId, Guid id, CancellationToken cancellationToken);

    ValueTask<DomainResult<HighLevelAttributeValue>> GetObjectAttribute(uint slotId, Guid id, string attributeName, CancellationToken cancellationToken);
    
    ValueTask<VoidDomainResult> SetObjectAttribute(uint slotId, Guid id, string attributeName, HighLevelAttributeValue value, CancellationToken cancellationToken);
}
