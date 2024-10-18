using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class MemorySessionExtensions
{
    public static IP11Session EnsureSession(this IMemorySession memorySession, uint sessionId)
    {
        if (!memorySession.TryGetSession(sessionId, out IP11Session? session))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_SESSION_HANDLE_INVALID, $"Invalid session handle {sessionId}.");
        }

        return session;
    }

    public static async ValueTask CheckIsSlotPluuged(this IMemorySession memorySession, uint sessionId, IP11HwServices hwServices, CancellationToken cancellationToken)
    {
        if (!memorySession.TryGetSession(sessionId, out IP11Session? session))
        {
            throw new RpcPkcs11Exception(Contracts.P11.CKR.CKR_SESSION_HANDLE_INVALID, $"Invalid session handle {sessionId}.");
        }

        _ = await hwServices.Persistence.EnsureSlot(session.SlotId, true, cancellationToken);
    }
}
