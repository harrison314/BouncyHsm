using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class VerifyRecoverHandler : IRpcRequestHandler<VerifyRecoverRequest, VerifyRecoverEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<VerifyRecoverHandler> logger;

    public VerifyRecoverHandler(IP11HwServices hwServices, ILogger<VerifyRecoverHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<VerifyRecoverEnvelope> Handle(VerifyRecoverRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        VerifyWithRecoveryState state = p11Session.State.Ensure<VerifyWithRecoveryState>();

        this.logger.LogDebug("Updating signature with signature length: {signatureLength}.", request.Signature.Length);

        bool isValid = state.Verify(request.Signature, out byte[]? recoveredMessage);

        if(isValid)
        {
            System.Diagnostics.Debug.Assert(recoveredMessage != null);

            if(request.IsPtrDataSet)
            {
                if (request.PulDataLen < (uint)recoveredMessage.Length)
                {
                    return new VerifyRecoverEnvelope()
                    {
                        Rv = (uint)CKR.CKR_BUFFER_TOO_SMALL,
                        Data = null
                    };
                }
            }
            else
            {
                return new VerifyRecoverEnvelope()
                {
                    Rv = (uint)CKR.CKR_OK,
                    Data = new VerifyRecoverData()
                    {
                       PulDataLen = (uint)recoveredMessage.Length
                    }
                };
            }
        }

        this.logger.LogInformation("Signature with recover in session {SessionId} is {signatureValidity}.",
            request.SessionId,
            isValid ? "valid" : "invalid");

        p11Session.ClearState();

        if (isValid)
        {
            System.Diagnostics.Debug.Assert(recoveredMessage != null);

            return new VerifyRecoverEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new VerifyRecoverData()
                {
                    Data = recoveredMessage,
                    PulDataLen = (uint)recoveredMessage.Length
                }
            };
        }
        else
        {
            return new VerifyRecoverEnvelope()
            {
                Rv = (uint)CKR.CKR_SIGNATURE_INVALID
            };
        }
    }
}