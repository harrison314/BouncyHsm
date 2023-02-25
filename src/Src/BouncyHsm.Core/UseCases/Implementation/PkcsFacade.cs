using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Core.UseCases.Implementation.Generators;
using Microsoft.Extensions.Logging;

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
}