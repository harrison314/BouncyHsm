using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SeedRandomHandler : IRpcRequestHandler<SeedRandomRequest, SeedRandomEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<SeedRandomHandler> logger;

    public SeedRandomHandler(IP11HwServices hwServices, ILogger<SeedRandomHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<SeedRandomEnvelope> Handle(SeedRandomRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IP11Session session = this.hwServices.ClientAppCtx.EnsureSession(request.AppId, request.SessionId);
        Contracts.Entities.SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(session.SlotId, cancellationToken);
        if (slot.Token == null)
        {
            if (slot.Token == null)
            {
                return new SeedRandomEnvelope()
                {
                    Rv = (uint)CKR.CKR_TOKEN_NOT_PRESENT
                };
            }
        }

        if (slot.Token.SimulateHwRng)
        {
            this.logger.LogWarning("Returns CKR_RANDOM_SEED_NOT_SUPPORTED.");
            return new SeedRandomEnvelope()
            {
                Rv = (uint)CKR.CKR_RANDOM_SEED_NOT_SUPPORTED
            };
        }
        else
        {
            session.SecureRandom.SetSeed(request.Seed);
            return new SeedRandomEnvelope()
            {
                Rv = (uint)CKR.CKR_OK
            };
        }
    }
}
