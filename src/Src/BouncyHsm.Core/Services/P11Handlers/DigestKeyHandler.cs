using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DigestKeyHandler : IRpcRequestHandler<DigestKeyRequest, DigestKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<DigestKeyHandler> logger;

    public DigestKeyHandler(IP11HwServices hwServices, ILogger<DigestKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DigestKeyEnvelope> Handle(DigestKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        DigestSessionState digestSessionState = p11Session.State.Ensure<DigestSessionState>();
        this.logger.LogDebug("Update digest using key and {sessionState}.", digestSessionState);

        StorageObject storageObject = await this.hwServices.FindObjectByHandle<StorageObject>(memorySession,
            p11Session,
            request.ObjectHandle,
            cancellationToken);

        if (storageObject is SecretKeyObject secretKeyObject)
        {
            digestSessionState.Update(secretKeyObject.GetSecret());
            this.logger.LogDebug("Update digest using key {keyId} - {keyType}.",
                secretKeyObject.Id,
                secretKeyObject);

            return new DigestKeyEnvelope()
            {
                Rv = (uint)CKR.CKR_OK
            };
        }
        else
        {
            this.logger.LogError("Object handle is not secret key. Returns CKR_KEY_INDIGESTIBLE.");
            return new DigestKeyEnvelope()
            {
                Rv = (uint)CKR.CKR_KEY_INDIGESTIBLE
            };
        }
    }
}
