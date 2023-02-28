using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation.Generators;
using BouncyHsm.Core.UseCases.Implementation.Visitors;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace BouncyHsm.Core.UseCases.Implementation;

public class PkcsFacade : IPkcsFacade
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ILogger<PkcsFacade> logger;

    public PkcsFacade(IPersistentRepository persistentRepository, ILogger<PkcsFacade> logger)
    {
        this.persistentRepository = persistentRepository;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<Guid>> ImportP12(ImportP12Request request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ImportP12");

        Services.Contracts.Entities.SlotEntity? slot = await this.persistentRepository.GetSlot(request.SlotId, cancellationToken);
        if (slot == null)
        {
            this.logger.LogError("SlotId {slotId} not found.", request.SlotId);
            return new DomainResult<Guid>.InvalidInput("SlotId not found.");
        }

        if (slot.Token == null)
        {
            this.logger.LogError("SlotId {slotId} canot contains token.", request.SlotId);
            return new DomainResult<Guid>.InvalidInput("Slot can not contains token.");
        }

        if (request.ImportMode == P12ImportMode.LocalInQualifiedArea && !slot.Token.SimulateQualifiedArea)
        {
            this.logger.LogError("SlotId {slotId} not contains token with qualified area.", request.SlotId);
            return new DomainResult<Guid>.InvalidInput("SlotId  not contains token with qualified area.");
        }

        try
        {
            P12ObjectsGenerator p12ObjectsGenerator = new P12ObjectsGenerator(request.Pkcs12Content,
                request.Password,
                request.CkaLabel,
                request.CkaId);
            p12ObjectsGenerator.ImportMode = request.ImportMode;

            List<StorageObject> objects = new List<StorageObject>
        {
            p12ObjectsGenerator.CreatePrivateKey(),
            p12ObjectsGenerator.CreatePublicKey(),
            p12ObjectsGenerator.CreateCertificate()
        };

            if (request.ImportChain)
            {
                objects.AddRange(p12ObjectsGenerator.GetCertificateChain());
            }

            foreach (StorageObject storageObject in objects)
            {
                storageObject.Validate();
            }


            foreach (StorageObject storageObject in objects)
            {
                await this.persistentRepository.StoreObject(request.SlotId,
                    storageObject,
                    cancellationToken);
            }

            this.logger.LogInformation("Succesfull imported Pkcs12 file into private key {privateKeyId}, publicKey {publicKeyId} and certificate {certificateId}.",
                objects[0].Id,
                objects[1].Id,
                objects[2].Id);

            return new DomainResult<Guid>.Ok(objects[0].Id);
        }
        catch (System.IO.IOException ex) when (ex.Message.Contains("MAC invalid"))
        {
            this.logger.LogError(ex, "Probably invalid password for P12.");
            return new DomainResult<Guid>.InvalidInput("Invalid password.");
        }
        catch (BouncyHsmInvalidInputException ex)
        {
            this.logger.LogError(ex, "Invalid input.");
            return new DomainResult<Guid>.InvalidInput(ex.Message);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexcepted exceptionduring import p12 file.");
            throw;
        }
    }

    public async ValueTask<DomainResult<PkcsObjects>> GetObjects(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetObjects with slotId {slotId}.", slotId);

        IEnumerable<PrivateKeyObject> privateKeys = await this.FindObjects<PrivateKeyObject>(slotId, CKO.CKO_PRIVATE_KEY, cancellationToken);
        IEnumerable<PublicKeyObject> publicKeys = await this.FindObjects<PublicKeyObject>(slotId, CKO.CKO_PUBLIC_KEY, cancellationToken);
        IEnumerable<X509CertificateObject> certificates = await this.FindObjects<X509CertificateObject>(slotId,
            CKO.CKO_CERTIFICATE,
            cancellationToken,
            new KeyValuePair<CKA, IAttributeValue>(CKA.CKA_CERTIFICATE_TYPE, AttributeValue.Create((uint)CKC.CKC_X_509)));

        StorageObjectDescriptionVisitor descriptionVisitor = new StorageObjectDescriptionVisitor();

        List<PkcsObjectInfo> objects = privateKeys.Select(t => new
        {
            key = new LabelIdPair(t.CkaLabel, t.CkaId),
            type = CKO.CKO_PRIVATE_KEY,
            id = t.Id,
            alwaysAuthenticate = t.CkaAlwaysAuthenticate,
            description = t.Accept(descriptionVisitor)
        })
            .Concat(publicKeys.Select(t => new
            {
                key = new LabelIdPair(t.CkaLabel, t.CkaId),
                type = CKO.CKO_PUBLIC_KEY,
                id = t.Id,
                alwaysAuthenticate = false,
                description = t.Accept(descriptionVisitor)
            }))
            .Concat(certificates.Select(t => new
            {
                key = new LabelIdPair(t.CkaLabel, t.CkaId),
                type = CKO.CKO_CERTIFICATE,
                id = t.Id,
                alwaysAuthenticate = false,
                description = t.Accept(descriptionVisitor)
            }))
            .GroupBy(t => t.key)
            .Select(t => new PkcsObjectInfo()
            {
                CkaLabel = t.Key.CkaLabel,
                CkaId = t.Key.CkaId,
                Objects = t.Select(q => new PkcsSpecificObject(q.type, q.id, q.description)).OrderByDescending(o => (uint)o.CkaClass).ToArray(),
                AlwaysAuthenticate = t.Any(q => q.alwaysAuthenticate)
            })
            .ToList();

        this.logger.LogTrace("Found {count} PKCS objects.", objects.Count);

        return new DomainResult<PkcsObjects>.Ok(new PkcsObjects(objects));
    }

    private async ValueTask<IEnumerable<T>> FindObjects<T>(uint slotId, CKO cko, CancellationToken cancellationToken, params KeyValuePair<CKA, IAttributeValue>[] additionalConstraints)
        where T : StorageObject
    {
        this.logger.LogTrace("Enetring to FindObjects with slotId {slotId}m cko {cko}.", slotId, cko);

        Dictionary<CKA, IAttributeValue> serachTemplate = new Dictionary<CKA, IAttributeValue>()
        {
            {CKA.CKA_CLASS, AttributeValue.Create((uint) cko) }
        };

        foreach ((CKA attrType, IAttributeValue attrValue) in additionalConstraints)
        {
            serachTemplate.Add(attrType, attrValue);
        }

        FindObjectSpecification specification = new FindObjectSpecification(serachTemplate, true);
        IReadOnlyList<StorageObject> result = await this.persistentRepository.FindObjects(slotId, specification, cancellationToken);
        return result.OfType<T>();
    }
}