using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class VerifyFinalHandler : IRpcRequestHandler<VerifyFinalRequest, VerifyFinalEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<VerifyFinalHandler> logger;

    public VerifyFinalHandler(IP11HwServices hwServices, ILogger<VerifyFinalHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<VerifyFinalEnvelope> Handle(VerifyFinalRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        VerifyState state = p11Session.State.Ensure<VerifyState>();

        bool isValid = state.Verify(request.Signature);

        this.logger.LogInformation("Signature in session {SessionId} is {signatureValidity}.",
            request.SessionId,
            isValid ? "valid" : "invalid");

            p11Session.ClearState();

        return new VerifyFinalEnvelope()
        {
            Rv = (uint)(isValid? CKR.CKR_OK: CKR.CKR_SIGNATURE_INVALID)
        };
    }
}