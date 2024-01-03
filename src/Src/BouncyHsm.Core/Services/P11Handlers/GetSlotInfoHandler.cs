using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetSlotInfoHandler : IRpcRequestHandler<GetSlotInfoRequest, GetSlotInfoEnvelope>
{
    private readonly IPersistentRepository persistence;
    private readonly ILogger<GetSlotInfoHandler> logger;

    public GetSlotInfoHandler(IPersistentRepository persistence, ILogger<GetSlotInfoHandler> logger)
    {
        this.persistence = persistence;
        this.logger = logger;
    }

    public async ValueTask<GetSlotInfoEnvelope> Handle(GetSlotInfoRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req SlotId {SlotId}.",
            request.SlotId);

        SlotEntity? slot = await this.persistence.GetSlot(request.SlotId, cancellationToken);

        if (slot == null)
        {
            this.logger.LogDebug("SlotId {SlotId} not found.", request.SlotId);
            return new GetSlotInfoEnvelope()
            {
                Rv = (uint)CKR.CKR_SLOT_ID_INVALID,
                Data = null
            };
        }

        CkVersion version = DataTransform.GetCurrentVersion();
        return new GetSlotInfoEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new SlotInfo()
            {
                SlotDescription = slot.Description,
                ManufacturerID = P11Constants.ManufacturerId,
                FlagsHwSlot = slot.IsHwDevice,
                FlagsRemovableDevice = false,
                FlagsTokenPresent = true, //TODO
                FirmwareVersion = version,
                HardwareVersion = version
            }
        };
    }
}
