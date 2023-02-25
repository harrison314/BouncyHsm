using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal record AuthenticatedSigner(ISigner Signer, bool RequireSignaturePin);