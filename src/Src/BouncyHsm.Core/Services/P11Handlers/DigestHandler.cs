using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DigestHandler : IRpcRequestHandler<DigestRequest, DigestEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<DigestHandler> logger;

    public DigestHandler(IP11HwServices hwServices, ILogger<DigestHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DigestEnvelope> Handle(DigestRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.",
          request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        DigestSessionState digestSessionState = p11Session.State.Ensure<DigestSessionState>();
        this.logger.LogDebug("Update digest using {sessionState}.", digestSessionState);

        if (digestSessionState.IsUpdated)
        {
            throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Invalid state of digest - is updated.");
        }

        if (request.IsDigestPtrSet)
        {
            if (request.PulDigestLen < digestSessionState.DigestLength)
            {
                throw new RpcPkcs11Exception(CKR.CKR_BUFFER_TOO_SMALL, $"Digest buffer is small ({request.PulDigestLen}, required is {digestSessionState.DigestLength}).");
            }

            digestSessionState.Update(request.Data);
            byte[] digest = digestSessionState.Final();
            p11Session.ClearState();

            return new DigestEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new DigestValue()
                {
                    Data = digest,
                    PulDigestLen = digestSessionState.DigestLength
                }
            };
        }
        else
        {
            return new DigestEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new DigestValue()
                {
                    Data = null,
                    PulDigestLen = digestSessionState.DigestLength
                }
            };
        }
    }
}
