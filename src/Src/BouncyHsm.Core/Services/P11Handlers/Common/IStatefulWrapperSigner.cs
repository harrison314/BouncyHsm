using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal interface IStatefulWrapperSigner : IWrapperSigner
{
    AuthenticatedStatefulSigner IntoStatefulSigner(KeyObject keyObject, IP11HwServices hwServices, IP11Session session);
}
