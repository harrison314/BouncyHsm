using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public interface ISlotFacade
{
    ValueTask<DomainResult<IReadOnlyList<SlotEntity>>> GetAllSlots(CancellationToken cancellationToken);

    ValueTask<DomainResult<CreateSlotResult>> CreateSlot(CreateSlotData createSlotData, CancellationToken cancellationToken);

    ValueTask<VoidDomainResult> DeleteSlot(uint slotId, CancellationToken cancellationToken);
}
