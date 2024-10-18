using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SignUpdateHandler : IRpcRequestHandler<SignUpdateRequest, SignUpdateEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<SignUpdateHandler> logger;

    public SignUpdateHandler(IP11HwServices hwServices, ILogger<SignUpdateHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<SignUpdateEnvelope> Handle(SignUpdateRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        SignState state = p11Session.State.Ensure<SignState>();

        if (state.RequiredUserLogin && !memorySession.IsUserLogged(p11Session.SlotId))
        {
            throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "User is not login.");
        }

        state.Update(request.Data);
        this.logger.LogDebug("Updating signature with data length: {dataLength}.", request.Data.Length);

        return new SignUpdateEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
