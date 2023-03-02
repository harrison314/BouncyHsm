using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class EncryptFinalHandler : IRpcRequestHandler<EncryptFinalRequest, EncryptFinalEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<EncryptFinalHandler> logger;

    public EncryptFinalHandler(IP11HwServices hwServices, ILogger<EncryptFinalHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public ValueTask<EncryptFinalEnvelope> Handle(EncryptFinalRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        EncryptState enctyptSessionState = p11Session.State.Ensure<EncryptState>();
        this.logger.LogDebug("Encrypt using {sessionState}.", enctyptSessionState);

        uint chiperTextLen = enctyptSessionState.GetFinalSize();

        if (request.IsEncryptedDataPtrSet)
        {
            if (request.EncryptedDataLen < chiperTextLen)
            {
                throw new RpcPkcs11Exception(CKR.CKR_BUFFER_TOO_SMALL, $"Encrypted data buffer is small ({request.EncryptedDataLen}, required is {chiperTextLen}).");
            }

            byte[] chiperText = enctyptSessionState.DoFinal();
            p11Session.ClearState();

            return new ValueTask<EncryptFinalEnvelope>(new EncryptFinalEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new EncryptData()
                {
                    EncryptedData = chiperText,
                    PullEncryptedDataLen = (uint)chiperText.Length
                }
            });
        }
        else
        {
            return new ValueTask<EncryptFinalEnvelope>(new EncryptFinalEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new EncryptData()
                {
                    EncryptedData = Array.Empty<byte>(),
                    PullEncryptedDataLen = chiperTextLen
                }
            });
        }
    }
}