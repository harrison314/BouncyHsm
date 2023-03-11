namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal enum BufferedCipherWrapperOperation
{
    CKA_ENCRYPT,
    CKA_DECRYPT,
    CKA_WRAP,
    CKA_UNWRAP
}