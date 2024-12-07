using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal record AuthenticatedSignerWithRecovery(ISignerWithRecovery Signer, bool RequireSignaturePin);