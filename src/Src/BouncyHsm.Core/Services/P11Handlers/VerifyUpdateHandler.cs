using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class VerifyUpdateHandler : IRpcRequestHandler<VerifyUpdateRequest, VerifyUpdateEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<VerifyUpdateHandler> logger;

    public VerifyUpdateHandler(IP11HwServices hwServices, ILogger<VerifyUpdateHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async Task<VerifyUpdateEnvelope> Handle(VerifyUpdateRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        VerifyState state = p11Session.State.Ensure<VerifyState>();

        state.Update(request.Data);
        this.logger.LogDebug("Updating signature with data length: {dataLength}.", request.Data.Length);

        return new VerifyUpdateEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
