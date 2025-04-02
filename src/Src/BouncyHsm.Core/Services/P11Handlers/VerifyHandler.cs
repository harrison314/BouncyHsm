using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class VerifyHandler : IRpcRequestHandler<VerifyRequest, VerifyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<VerifyHandler> logger;

    public VerifyHandler(IP11HwServices hwServices, ILogger<VerifyHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<VerifyEnvelope> Handle(VerifyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        VerifyState state = p11Session.State.Ensure<VerifyState>();

        state.Update(request.Data);
        this.logger.LogDebug("Updating signature with data length: {dataLength}.", request.Data.Length);

        bool isValid = state.Verify(request.Signature);

        this.logger.LogInformation("Signature in session {SessionId} is {signatureValidity}.",
            request.SessionId,
            isValid ? "valid" : "invalid");

        p11Session.ClearState();

        return new VerifyEnvelope()
        {
            Rv = (uint)(isValid ? CKR.CKR_OK : CKR.CKR_SIGNATURE_INVALID)
        };
    }
}
