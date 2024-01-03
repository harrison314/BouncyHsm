using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GenerateRandomHandler : IRpcRequestHandler<GenerateRandomRequest, GenerateRandomEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GenerateRandomHandler> logger;

    public GenerateRandomHandler(IP11HwServices hwServices, ILogger<GenerateRandomHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<GenerateRandomEnvelope> Handle(GenerateRandomRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IP11Session session = this.hwServices.ClientAppCtx.EnsureSession(request.AppId, request.SessionId);
        Contracts.Entities.SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(session.SlotId, cancellationToken);

        int length = Convert.ToInt32(request.RandomLen);
        byte[] data = new byte[length];

        session.SecureRandom.NextBytes(data);

        return new GenerateRandomEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = data
        };
    }
}