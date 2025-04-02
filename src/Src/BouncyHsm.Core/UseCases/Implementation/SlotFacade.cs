using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Utils;
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class SlotFacade : ISlotFacade
{
    private readonly IPersistentRepository persistentRepository;
    private readonly IClientApplicationContext clientAppCtx;
    private readonly ILogger<SlotFacade> logger;

    public SlotFacade(IPersistentRepository persistentRepository, IClientApplicationContext clientAppCtx, ILogger<SlotFacade> logger)
    {
        this.persistentRepository = persistentRepository;
        this.clientAppCtx = clientAppCtx;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<CreateSlotResult>> CreateSlot(CreateSlotData createSlotData, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to CreateSlot");

        SlotEntity slotEntity = new SlotEntity()
        {
            Description = createSlotData.Description.Trim(),
            IsHwDevice = createSlotData.IsHwDevice,
            IsRemovableDevice = createSlotData.IsRemovableDevice,
            IsUnplugged = false,
            Token = new TokenInfo()
            {
                IsSoPinLocked = false,
                IsUserPinLocked = false,
                Label = createSlotData.Token.Label.Trim(),
                SerialNumber = createSlotData.Token.SerialNumber?.Trim() ?? this.CreateSerial(),
                SimulateHwMechanism = createSlotData.Token.SimulateHwMechanism,
                SimulateHwRng = createSlotData.Token.SimulateHwRng,
                SimulateQualifiedArea = createSlotData.Token.SimulateQualifiedArea,
                SimulateProtectedAuthPath = createSlotData.Token.SimulateProtectedAuthPath,
                SpeedMode = createSlotData.Token.SpeedMode
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
        catch (BouncyHsmStorageException ex) when (ex.Message.Contains("already exists"))
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

    public async ValueTask<DomainResult<SlotEntity>> GetSlotById(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlotById with slotId {slotId}.", slotId);

        SlotEntity? result = await this.persistentRepository.GetSlot(slotId, cancellationToken);
        if (result == null)
        {
            this.logger.LogError("Slot with slotId {slotId} not found.", slotId);
            return new DomainResult<SlotEntity>.NotFound();
        }

        return new DomainResult<SlotEntity>.Ok(result);
    }

    public async ValueTask<VoidDomainResult> DeleteSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DeleteSlot with slotId {slotId}.", slotId);
        try
        {
            await this.persistentRepository.DeleteSlot(slotId, cancellationToken);
            this.logger.LogInformation("Removed slot with slotId {slotId}.", slotId);

            return new VoidDomainResult.Ok();
        }
        catch (BouncyHsmNotFoundException ex)
        {
            this.logger.LogError(ex, "Slot {slotId} not found.", slotId);
            return new VoidDomainResult.NotFound();
        }
    }

    public async ValueTask<VoidDomainResult> SetPluggedState(uint slotId, bool plugged, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to SetPluggedState with slotId {slotId}, plugged {plugged}.", slotId, plugged);
        try
        {
            SlotEntity? slot = await this.persistentRepository.GetSlot(slotId, cancellationToken);
            if (slot == null)
            {
                this.logger.LogError("Slot with slotId {slotId} not found.", slotId);
                return new VoidDomainResult.NotFound();
            }

            if (!slot.IsRemovableDevice)
            {
                this.logger.LogError("Slot with slotId {slotId} is not removable device.", slotId);
                return new VoidDomainResult.InvalidInput("Slot is not removable device.");
            }

            if (slot.IsUnplugged != plugged)
            {
                this.logger.LogDebug("Slot with slotId {slotId} already set IsUnplugged to {isUnplugged}.", slotId, slot.IsUnplugged);
                return new VoidDomainResult.Ok();
            }

            bool slotChanged = await this.persistentRepository.ExecuteSlotCommand(slotId,
                plugged ? new SlotCommands.PlugDeviceCommand() : new SlotCommands.UnplugDeviceCommand(),
                cancellationToken);

            if (slotChanged)
            {
                this.clientAppCtx.NotifySlotEvent(slotId);
                this.logger.LogInformation("Slot with slotId {slotId} set IsUnplugged to {isUnplugged}.", slotId, slot.IsUnplugged);
            }

            return new VoidDomainResult.Ok();
        }
        catch (BouncyHsmNotFoundException ex)
        {
            this.logger.LogError(ex, "Slot {slotId} not found.", slotId);
            return new VoidDomainResult.NotFound();
        }
    }

    public async ValueTask<VoidDomainResult> SetTokenPin(uint slotId, SetTokenPinData pinData, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to SetTokenPin with slotId {slotId}, userType {userType}.", slotId, pinData.UserType);
        try
        {
            SlotEntity? slot = await this.persistentRepository.GetSlot(slotId, cancellationToken);
            if (slot == null)
            {
                this.logger.LogError("Slot with slotId {slotId} not found.", slotId);
                return new VoidDomainResult.NotFound();
            }

            if (string.IsNullOrEmpty(pinData.NewPin))
            {
                this.logger.LogError("Pin is empty string for SetTokenPin with slotId {slotId}", slotId);
                return new VoidDomainResult.InvalidInput("Pin can not by empty string.");
            }

            if (pinData.UserType == Services.Contracts.P11.CKU.CKU_CONTEXT_SPECIFIC && !slot.Token.SimulateQualifiedArea)
            {
                this.logger.LogError("Slot with slotId {slotId} can not simulate qualified area - can not use CKU_CONTEXT_SPECIFIC user type.", slotId);
                return new VoidDomainResult.InvalidInput("Slot can not simulate qualified area - can not use CKU_CONTEXT_SPECIFIC user type.");
            }

            await this.persistentRepository.SetPin(slot,
                pinData.UserType,
                pinData.NewPin,
                null,
                cancellationToken);

            this.logger.LogInformation("New pin has set for token with slotId {slotId} and user type {userType}.", slotId, pinData.UserType);
            return new VoidDomainResult.Ok();
        }
        catch (BouncyHsmNotFoundException ex)
        {
            this.logger.LogError(ex, "Slot {slotId} not found.", slotId);
            return new VoidDomainResult.NotFound();
        }
    }


    private string CreateSerial(int serialLen = 8)
    {
        Span<byte> data = (serialLen <= 512) ? stackalloc byte[serialLen] : new byte[serialLen];
        RandomNumberGenerator.Fill(data);

        return HexConvertor.GetString(data);
    }
}
