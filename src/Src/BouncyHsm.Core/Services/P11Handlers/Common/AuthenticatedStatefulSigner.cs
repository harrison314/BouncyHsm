namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal record AuthenticatedStatefulSigner(IStatefulSigner Signer, bool RequireSignaturePin);
