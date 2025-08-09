using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetSlotListHandler : IRpcRequestHandler<GetSlotListRequest, GetSlotListEnvelope>
{
    private readonly IPersistentRepository persistence;
    private readonly ILogger<GetSlotListHandler> logger;

    public GetSlotListHandler(IPersistentRepository persistence, ILogger<GetSlotListHandler> logger)
    {
        this.persistence = persistence;
        this.logger = logger;
    }

    public async Task<GetSlotListEnvelope> Handle(GetSlotListRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req IsTokenPresent {IsTokenPresent}, PullCount {PullCount}, IsSlotListPointerPresent {IsSlotListPointerPresent}.",
            request.IsTokenPresent, request.PullCount, request.IsSlotListPointerPresent);

        IReadOnlyList<Contracts.Entities.SlotEntity> slots = await this.persistence.GetSlots(new GetSlotSpecification(request.IsTokenPresent), cancellationToken);
        uint[] slotIds = slots.Select(t => t.SlotId).ToArray();

        GetSlotListEnvelope envelope = new GetSlotListEnvelope();
        if (request.IsSlotListPointerPresent)
        {
            if (request.PullCount < slotIds.Length)
            {
                envelope.Slots = Array.Empty<uint>();
                envelope.PullCount = 0;
                envelope.Rv = (uint)CKR.CKR_BUFFER_TOO_SMALL;
            }
            else
            {
                envelope.Slots = slotIds;
                envelope.PullCount = (uint)slotIds.Length;
                envelope.Rv = (uint)CKR.CKR_OK;
            }
        }
        else
        {
            envelope.Slots = Array.Empty<uint>();
            envelope.PullCount = (uint)slotIds.Length;
            envelope.Rv = (uint)CKR.CKR_OK;
        }

        return envelope;
    }
}