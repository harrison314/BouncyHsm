using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal interface IWrapperSignWithRecover
{
    AuthenticatedSignerWithRecovery IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom);

    ISignerWithRecovery IntoValidationSigner(KeyObject keyObject);
}
