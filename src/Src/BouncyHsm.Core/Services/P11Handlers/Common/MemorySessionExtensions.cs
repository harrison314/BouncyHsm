using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class MemorySessionExtensions
{
    public static IP11Session EnsureSession(this IMemorySession memorySession, uint sessionId)
    {
        if(!memorySession.TryGetSession(sessionId, out IP11Session? session))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_SESSION_HANDLE_INVALID, $"Invalid session handle {sessionId}.");
        }

        return session;
    }
}
