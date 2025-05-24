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

    public async ValueTask<DomainResult<HighLevelAttributeValue>> GetObjectAttribute(uint slotId, Guid id, string attributeName, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetObjectAttribute with slotId {slotId}, id {id}, attributeName {attributeName}.", slotId, id, attributeName);

        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, id, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Storage object with id {id} not found.", id);
            return new DomainResult<HighLevelAttributeValue>.NotFound();
        }

        if (!Enum.TryParse<CKA>(attributeName, out CKA attributeType))
        {
            this.logger.LogError("Attribute {attributeName} is not a valid CKA.", attributeName);
            return new DomainResult<HighLevelAttributeValue>.InvalidInput($"Attribute {attributeName} is not a valid CKA.");
        }

        StorageObjectMemento memento = storageObject.ToMemento();
        if (!memento.Values.TryGetValue(attributeType, out IAttributeValue? value))
        {
            this.logger.LogError("Storage object with id {id} does not contain attribute {attributeName}.", id, attributeName);
            return new DomainResult<HighLevelAttributeValue>.NotFound();
        }

        return new DomainResult<HighLevelAttributeValue>.Ok(new HighLevelAttributeValue(value));
    }

    public async ValueTask<VoidDomainResult> SetObjectAttribute(uint slotId, Guid id, string attributeName, HighLevelAttributeValue value, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to SetObjectAttribute with slotId {slotId}, id {id}, attributeName {attributeName}.", slotId, id, attributeName);
        StorageObject? storageObject = await this.persistentRepository.TryLoadObject(slotId, id, cancellationToken);
        if (storageObject == null)
        {
            this.logger.LogError("Storage object with id {id} not found.", id);
            return new VoidDomainResult.NotFound();
        }

        if (!Enum.TryParse<CKA>(attributeName, out CKA attributeType))
        {
            this.logger.LogError("Attribute {attributeName} is not a valid CKA.", attributeName);
            return new VoidDomainResult.InvalidInput($"Attribute {attributeName} is not a valid CKA.");
        }

        if (attributeType is CKA.CKA_CLASS or CKA.CKA_TOKEN or CKA.CKA_KEY_TYPE)
        {
            this.logger.LogError("Attribute {attributeName} cannot be set.", attributeName);
            return new VoidDomainResult.InvalidInput($"Attribute {attributeName} cannot be set.");
        }

        StorageObjectMemento memento = storageObject.ToMemento();
        if (!memento.Values.TryGetValue(attributeType, out IAttributeValue? oldValue))
        {
            this.logger.LogError("Storage object with id {id} does not contain attribute {attributeName}.", id, attributeName);
            return new VoidDomainResult.NotFound();
        }

        try
        {
            IAttributeValue newValue = value.ToAttributeValue();
            if (newValue.TypeTag != oldValue.TypeTag)
            {
                this.logger.LogError("Attribute {attributeName} has incompatible type: expected {expectedType}, got {actualType}.", attributeName, oldValue.TypeTag, newValue.TypeTag);
                return new VoidDomainResult.InvalidInput($"Attribute {attributeName} has incompatible type: expected {oldValue.TypeTag}, got {newValue.TypeTag}.");
            }

            memento.Values[attributeType] = newValue;
        }
        catch (InvalidDataException ex)
        {
            this.logger.LogError(ex, "Failed to set attribute {attributeName} for storage object with id {id}.", attributeName, id);
            return new VoidDomainResult.InvalidInput($"Failed to set attribute {attributeName}: {ex.Message}");
        }

        StorageObject newStorageObject = StorageObjectFactory.CreateFromMemento(memento);

        try
        {
            newStorageObject.Validate();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to create new storage object from memento for id {id}.", id);
            return new VoidDomainResult.InvalidInput($"Failed to create new storage object: {ex.Message}");
        }

        await this.persistentRepository.UpdateObject(slotId, newStorageObject, cancellationToken);
        this.logger.LogInformation("Storage object with id {id} updated successfully.", id);

        return new VoidDomainResult.Ok();
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
