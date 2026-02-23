using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal record Ecdh1DeriveParamsWithoutPublicKey(CKD Kdf, byte[]? PublicKeyData, byte[]? SharedData);
