using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation.SlotCommands;
using BouncyHsm.Core.UseCases.Implementation.Visitors;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.UseCases.Implementation;

public class MigrationFacade : IMigrationFacade
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<MigrationFacade> logger;

    public MigrationFacade(IP11HwServices hwServices, ILogger<MigrationFacade> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<MigrationResult>> Migrate(MigrationRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Migrate");

        IReadOnlyList<SlotEntity> slots = await this.hwServices.Persistence.GetSlots(new GetSlotSpecification(false), cancellationToken);
        FindObjectSpecification specification = new FindObjectSpecification(new Dictionary<CKA, IAttributeValue>(), true);

        foreach (SlotEntity slot in slots)
        {
            this.logger.LogTrace("Migrate slot {SlotId} <{TokenLabel}>.", slot.SlotId, slot.Token.Label);
            await this.hwServices.Persistence.ExecuteSlotCommand(slot.SlotId, new MigrateSlotCommand(), cancellationToken);
        }

        int successed = 0;
        int failed = 0;
        MigrateObjectFlags migrationFlags = this.CreateMigrationFlags(request);
        foreach (SlotEntity slot in slots)
        {
            this.logger.LogTrace("Process slot {SlotId} <{TokenLabel}>.", slot.SlotId, slot.Token.Label);

            IReadOnlyList<StorageObject> objects = await this.hwServices.Persistence.FindObjects(slot.SlotId, specification, cancellationToken);

            foreach (StorageObject storageObject in objects)
            {
                this.logger.LogTrace("Process object with id {ObjectId} and label <{ObjectLabel}>",
                    storageObject.Id,
                    storageObject.CkaLabel);

                try
                {
                    StorageObject newStorageObject = this.AddMissingAttributes(storageObject);
                    newStorageObject.MigrateObject(migrationFlags);
                    newStorageObject.ReComputeAttributes();
                    newStorageObject.Validate();

                    await this.hwServices.Persistence.UpdateObject(slot.SlotId, newStorageObject, cancellationToken);
                    successed++;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error during migration object in slot {SlotId} with Id {ObjectId}, label <{ObjectLabel}> and type {ObjectType}.",
                        slot.SlotId,
                        storageObject.Id,
                        storageObject.CkaLabel,
                        this.GetObjectDescriptionSafe(storageObject));
                    failed++;
                }
            }
        }

        this.logger.LogInformation("Migration status: successed: {Successed}, failed: {Failed}.", successed, failed);
        return new DomainResult<MigrationResult>.Ok(new MigrationResult(successed, failed));
    }

    private MigrateObjectFlags CreateMigrationFlags(MigrationRequest request)
    {
        MigrateObjectFlags flags = MigrateObjectFlags.None;
        if (request.ResetAllowedMechanism)
        {
            flags |= MigrateObjectFlags.ResetAlowedMechanism;
        }

        return flags;
    }

    private string GetObjectDescriptionSafe(StorageObject storageObject)
    {
        try
        {
            StorageObjectDescriptionVisitor nameVisitor = new StorageObjectDescriptionVisitor();
            return storageObject.Accept(nameVisitor);
        }
        catch
        {
            return "-";
        }
    }

    private StorageObject AddMissingAttributes(StorageObject storageObject)
    {
        this.logger.LogTrace("Entering to AddMissingAttributes");

        StorageObjectMemento memento = storageObject.ToMemento();
        StorageObject prototype = StorageObjectFactory.CreateEmpty(memento.Values);

        bool attributesAdded = false;
        foreach ((CKA attrType, IAttributeValue attrValue) in prototype.ToMemento().Values)
        {
            if (!memento.Values.ContainsKey(attrType))
            {
                memento.Values.Add(attrType, attrValue);
                attributesAdded = true;

                this.logger.LogDebug("Add default attribute {Attribute} into object {StorageObjectId}.",
                    attrType,
                    memento.Id);
            }
        }

        if (attributesAdded)
        {
            return StorageObjectFactory.CreateFromMemento(memento, false);
        }
        else
        {
            return storageObject;
        }
    }
}