using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class ClientApplicationContextExtensions
{
    public static IMemorySession EnsureMemorySession(this IClientApplicationContext ctx, AppIdentification appIdentification)
    {
        if (!ctx.TryGetMemorySession(DataTransform.GetApplicationKey(appIdentification), out IMemorySession? memorySession))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_CRYPTOKI_NOT_INITIALIZED, $"Application with nonce {appIdentification.AppNonce} and pid {appIdentification.Pid} not initialized.");
        }

        return memorySession;
    }

    public static IP11Session EnsureSession(this IClientApplicationContext ctx, AppIdentification appIdentification, uint sessionId)
    {
        if (!ctx.TryGetMemorySession(DataTransform.GetApplicationKey(appIdentification), out IMemorySession? memorySession))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_CRYPTOKI_NOT_INITIALIZED, $"Application with nonce {appIdentification.AppNonce} and pid {appIdentification.Pid} not initialized.");
        }

        if (!memorySession.TryGetSession(sessionId, out IP11Session? session))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_SESSION_HANDLE_INVALID, $"Invalid session handle {sessionId}.");
        }

        return session;
    }
}
