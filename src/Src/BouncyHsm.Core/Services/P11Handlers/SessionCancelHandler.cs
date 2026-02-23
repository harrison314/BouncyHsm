using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SessionCancelHandler : IRpcRequestHandler<SessionCancelRequest, SessionCancelEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<SessionCancelHandler> logger;

    public SessionCancelHandler(IP11HwServices hwServices, ILogger<SessionCancelHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async Task<SessionCancelEnvelope> Handle(SessionCancelRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} flags {ckfFlags}.",
            request.SessionId,
            request.CkfFlags);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        ISessionState sessionState = p11Session.State;
        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_DECRYPT) && sessionState is DecryptState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_DECRYPT));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_ENCRYPT) && sessionState is EncryptState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_ENCRYPT));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_DIGEST) && sessionState is DigestSessionState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_DIGEST));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_SIGN) && sessionState is SignState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_SIGN));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_SIGN_RECOVER) && sessionState is SignWithRecoverState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_SIGN_RECOVER));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_VERIFY) && sessionState is VerifyState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_VERIFY));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_VERIFY_RECOVER) && sessionState is VerifyWithRecoveryState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_VERIFY_RECOVER));
            p11Session.ClearState();
        }

        if (this.IsFlagSet(request.CkfFlags, CKF.CKF_FIND_OBJECTS) && sessionState is FindObjectsState)
        {
            this.logger.LogInformation("Clear session {sessionId} state {stateName}", p11Session.SessionId, nameof(CKF.CKF_FIND_OBJECTS));
            p11Session.ClearState();
        }

        return new SessionCancelEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFlagSet(uint ckfFlags, uint flag)
    {
        return (ckfFlags & flag) == flag;
    }
}