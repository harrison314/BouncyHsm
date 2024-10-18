using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class EncryptHandler : IRpcRequestHandler<EncryptRequest, EncryptEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<EncryptHandler> logger;

    public EncryptHandler(IP11HwServices hwServices, ILogger<EncryptHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<EncryptEnvelope> Handle(EncryptRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        EncryptState encryptSessionState = p11Session.State.Ensure<EncryptState>();
        this.logger.LogDebug("Encrypt using {sessionState}.", encryptSessionState);

        uint cipherTextLen = encryptSessionState.GetFinalSize(request.Data);

        if (request.IsEncryptedDataPtrSet)
        {
            if (request.EncryptedDataLen < cipherTextLen)
            {
                throw new RpcPkcs11Exception(CKR.CKR_BUFFER_TOO_SMALL, $"Encrypted data buffer is small ({request.EncryptedDataLen}, required is {cipherTextLen}).");
            }

            byte[] cipherText = encryptSessionState.DoFinal(request.Data);
            p11Session.ClearState();

            return new EncryptEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new EncryptData()
                {
                    EncryptedData = cipherText,
                    PullEncryptedDataLen = (uint)cipherText.Length
                }
            };
        }
        else
        {
            return new EncryptEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new EncryptData()
                {
                    EncryptedData = Array.Empty<byte>(),
                    PullEncryptedDataLen = cipherTextLen
                }
            };
        }
    }
}
