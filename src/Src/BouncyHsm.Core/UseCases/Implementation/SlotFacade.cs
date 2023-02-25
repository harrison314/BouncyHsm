using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Utils;
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class SlotFacade : ISlotFacade
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ILogger<SlotFacade> logger;

    public SlotFacade(IPersistentRepository persistentRepository, ILogger<SlotFacade> logger)
    {
        this.persistentRepository = persistentRepository;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<CreateSlotResult>> CreateSlot(CreateSlotData createSlotData, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to CreateSlot");

        SlotEntity slotEntity = new SlotEntity()
        {
            Description = createSlotData.Description.Trim(),
            IsHwDevice = createSlotData.IsHwDevice,
            Token = new TokenInfo()
            {
                IsSoPinLocked = false,
                IsUserPinLocked = false,
                Label = createSlotData.Token.Label.Trim(),
                SerialNumber = createSlotData.Token.SerialNumber?.Trim() ?? this.CreateSerial(),
                SimulateHwMechanism = createSlotData.Token.SimulateHwMechanism,
                SimulateHwRng = createSlotData.Token.SimulateHwRng,
                SimulateQualifiedArea = createSlotData.Token.SimulateQualifiedArea
            }
        };

        TokenPins tokenPins = new TokenPins(createSlotData.Token.UserPin,
            createSlotData.Token.SoPin,
            createSlotData.Token.SignaturePin);

        if (createSlotData.Token.SimulateQualifiedArea && string.IsNullOrEmpty(createSlotData.Token.SignaturePin))
        {
            this.logger.LogError("Require signature PIN for Qualified Area.");
            return new DomainResult<CreateSlotResult>.InvalidInput("Require signature PIN for Qualified Area.");
        }

        try
        {
            SlotIds isd = await this.persistentRepository.CreateSlot(slotEntity, tokenPins, cancellationToken);

            this.logger.LogInformation("Create new slot with Id {Id} and SlotId {SlotId}.", isd.Id, isd.SlotId);
            CreateSlotResult result = new CreateSlotResult(isd.Id,
                isd.SlotId,
                slotEntity.Token.SerialNumber);

            return new DomainResult<CreateSlotResult>.Ok(result);
        }
        catch (BouncyHsmStorageException ex) when (ex.Message.Contains("alerady exists"))
        {
            this.logger.LogError(ex, "Error during create slot.");
            return new DomainResult<CreateSlotResult>.InvalidInput(ex.Message);
        }
    }

    public async ValueTask<DomainResult<IReadOnlyList<SlotEntity>>> GetAllSlots(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetAllSlots");

        IReadOnlyList<SlotEntity> result = await this.persistentRepository.GetSlots(new GetSlotSpecification(false), cancellationToken);
        return new DomainResult<IReadOnlyList<SlotEntity>>.Ok(result);
    }

    private string CreateSerial(int serialLen = 8)
    {
        Span<byte> data = (serialLen <= 512) ? stackalloc byte[serialLen] : new byte[serialLen];
        RandomNumberGenerator.Fill(data);

        return HexConvertor.GetString(data);
    }
}
