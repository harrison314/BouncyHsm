using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

public static class SessionStateExtensions
{
    public static void EnsureEmpty(this ISessionState state)
    {
        if (state is not EmptySessionState)
        {
            throw new RpcPkcs11Exception(CKR.CKR_OPERATION_ACTIVE, $"Actual session state is {state.GetType().Name}");
        }
    }

    public static T Ensure<T>(this ISessionState state)
        where T : ISessionState
    {
        if (state is T concreteState)
        {
            return concreteState;
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_OPERATION_NOT_INITIALIZED, $"Required another session state. Actual is {state.GetType().Name} required {typeof(T).Name}.");
        }
    }
}