using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.UseCases.Contracts;

public interface ISlotFacade
{
    ValueTask<DomainResult<IReadOnlyList<SlotEntity>>> GetAllSlots(CancellationToken cancellationToken);

    ValueTask<DomainResult<CreateSlotResult>> CreateSlot(CreateSlotData createSlotData, CancellationToken cancellationToken);

    ValueTask<DomainResult<SlotEntity>> GetSlotById(uint slotId, CancellationToken cancellationToken);

    ValueTask<VoidDomainResult> DeleteSlot(uint slotId, CancellationToken cancellationToken);

    ValueTask<VoidDomainResult> SetPluggedState(uint slotId, bool plugged, CancellationToken cancellationToken);
    
    ValueTask<VoidDomainResult> SetTokenPin(uint slotId, SetTokenPinData pinData, CancellationToken cancellationToken);
}
