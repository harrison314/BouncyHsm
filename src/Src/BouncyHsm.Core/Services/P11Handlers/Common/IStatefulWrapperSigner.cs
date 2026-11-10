using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal interface IStatefulWrapperSigner : IWrapperSigner
{
    //TODO: refactor to union return type for method IntoSigningSigner
    AuthenticatedStatefulSigner IntoStatefulSigner(KeyObject keyObject, IMemorySession memorySession, IP11HwServices hwServices, IP11Session session);
}
