using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class EncryptUpdateHandler : IRpcRequestHandler<EncryptUpdateRequest, EncryptUpdateEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<EncryptUpdateHandler> logger;

    public EncryptUpdateHandler(IP11HwServices hwServices, ILogger<EncryptUpdateHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<EncryptUpdateEnvelope> Handle(EncryptUpdateRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        EncryptState encryptSessionState = p11Session.State.Ensure<EncryptState>();
        this.logger.LogDebug("Encrypt using {sessionState}.", encryptSessionState);

        uint cipherTextLen = encryptSessionState.GetUpdateSize(request.PartData);

        if (request.IsEncryptedDataPtrSet)
        {
            if (request.EncryptedDataLen < cipherTextLen)
            {
                throw new RpcPkcs11Exception(CKR.CKR_BUFFER_TOO_SMALL, $"Encrypted data buffer is small ({request.EncryptedDataLen}, required is {cipherTextLen}).");
            }

            byte[] cipherText = encryptSessionState.Update(request.PartData);

            return new EncryptUpdateEnvelope()
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
            return new EncryptUpdateEnvelope()
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
