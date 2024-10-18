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

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        Contracts.Entities.SlotEntity slot = await this.hwServices.Persistence.EnsureSlot(p11Session.SlotId, true, cancellationToken);

        int length = Convert.ToInt32(request.RandomLen);
        byte[] data = new byte[length];

        p11Session.SecureRandom.NextBytes(data);

        return new GenerateRandomEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = data
        };
    }
}