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
}
