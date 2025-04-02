using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DecryptFinalHandler : IRpcRequestHandler<DecryptFinalRequest, DecryptFinalEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<DecryptFinalHandler> logger;

    public DecryptFinalHandler(IP11HwServices hwServices, ILogger<DecryptFinalHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DecryptFinalEnvelope> Handle(DecryptFinalRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        DecryptState decryptSessionState = p11Session.State.Ensure<DecryptState>();
        this.logger.LogDebug("Decrypt using {sessionState}.", decryptSessionState);

        uint plainTextLen = decryptSessionState.GetFinalSize();

        if (request.IsDataPtrSet)
        {
            if (request.PullDataLen < plainTextLen)
            {
                throw new RpcPkcs11Exception(CKR.CKR_BUFFER_TOO_SMALL, $"Decrypt data buffer is small ({request.PullDataLen}, required is {plainTextLen}).");
            }

            byte[] plainText = decryptSessionState.DoFinal();
            p11Session.ClearState();

            return new DecryptFinalEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new DecryptData()
                {
                    Data = plainText,
                    PullDataLen = (uint)plainText.Length
                }
            };
        }
        else
        {
            return new DecryptFinalEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new DecryptData()
                {
                    Data = Array.Empty<byte>(),
                    PullDataLen = plainTextLen
                }
            };
        }
    }
}