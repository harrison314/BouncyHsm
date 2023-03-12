using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Implementation.Visitors;
using BouncyHsm.Core.Services.Utils;

namespace BouncyHsm.Core.UseCases.Implementation;

public class StorageObjectsFacade : IStorageObjectsFacade
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ILogger<StorageObjectsFacade> logger;

    public StorageObjectsFacade(IPersistentRepository persistentRepository, ILogger<StorageObjectsFacade> logger)
    {
        this.persistentRepository = persistentRepository;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<StorageObjectsList>> GetStorageObjects(uint slotId, int skip, int take, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetStorageObjects with slotId {slotId}, skip {skip}, take {take}.", slotId, skip, take);

        FindObjectSpecification specification = new FindObjectSpecification(new Dictionary<CKA, IAttributeValue>(), true);
        IReadOnlyList<StorageObject> allObjects = await this.persistentRepository.FindObjects(slotId, specification, cancellationToken);

        StorageObjectDescriptionVisitor visitor = new StorageObjectDescriptionVisitor();

        StorageObjectsList list = new StorageObjectsList()
        {
            TotalCount = allObjects.Count,
            Objects = allObjects.Skip(skip).Take(take).Select(t => this.BuildInfo(t, t.Accept(visitor))).ToList()
        };

        return new DomainResult<StorageObjectsList>.Ok(list);
    }

    public async ValueTask<DomainResult<StorageObjectDetail>> GetStorageObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetStorageObject with slotId {slotId}, id {id}.", slotId, id);

        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, id, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Storage object with id {id} not found.", id);
            return new DomainResult<StorageObjectDetail>.NotFound();
        }

        StorageObjectDetail detail = new StorageObjectDetail()
        {
            Id = storageObject.Id,
            Description = storageObject.Accept(new StorageObjectDescriptionVisitor()),
            Attributes = new List<StorageObjectAttribute>()
        };

        StorageObjectMemento memento = storageObject.ToMemento();
        foreach (KeyValuePair<CKA, IAttributeValue> kvp in memento.Values)
        {
            StorageObjectAttribute attrInfo = new StorageObjectAttribute()
            {
                AttributeType = kvp.Key,
                TypeTag = kvp.Value.TypeTag,
                Size = (int)kvp.Value.GuessSize(),
                ValueHex = kvp.Value.ToHexadecimal(),
                ValueText = kvp.Value.ToPrintable(kvp.Key)
            };

            detail.Attributes.Add(attrInfo);
        }

        return new DomainResult<StorageObjectDetail>.Ok(detail);
    }

    public async ValueTask<VoidDomainResult> DeleteStorageObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DeleteStorageObject with slotId {slotId}, id {id}.", slotId, id);

        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, id, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Storage object with id {id} not found.", id);
            return new VoidDomainResult.NotFound();
        }

        await this.persistentRepository.DestroyObject(slotId, storageObject, cancellationToken);
        return new VoidDomainResult.Ok();
    }

    public async ValueTask<DomainResult<ObjectContent>> Download(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Download with slotId {slotId}, id {id}.", slotId, id);

        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, id, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Storage object with id {id} not found.", id);
            return new DomainResult<ObjectContent>.NotFound();
        }

        return storageObject.Accept(new ObjectContentVisitor());
    }

    private StorageObjectInfo BuildInfo(StorageObject storageObject, string description)
    {
        string? ckaIdHex = storageObject.GetValue(CKA.CKA_ID)
            .MatchOk<string?>(ok => HexConvertor.GetString(ok.Value.AsByteArray()),
            () => null);

        CKK? keyType = storageObject.GetValue(CKA.CKA_KEY_TYPE)
            .MatchOk<CKK?>(ok => (CKK)ok.Value.AsUint(),
            () => null);

        return new StorageObjectInfo()
        {
            Id = storageObject.Id,
            CkLabel = storageObject.CkaLabel,
            CkIdHex = ckaIdHex,
            Description = description,
            KeyType = keyType,
            Type = storageObject.CkaClass
        };
    }
}
