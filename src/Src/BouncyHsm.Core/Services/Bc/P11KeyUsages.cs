namespace BouncyHsm.Core.Services.Bc;

public record P11KeyUsages(bool CanSignAndVerify, bool CanEncryptAndDecrypt, bool CanDerive);