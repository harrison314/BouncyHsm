using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal interface IWrapperSigner
{
    AuthenticatedSigner IntoSigningSigner(KeyObject keyObject, SecureRandom secureRandom);

    ISigner IntoValidationSigner(KeyObject keyObject);
}
