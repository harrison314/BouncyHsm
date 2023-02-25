using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal record Ecdh1DeriveParams(CKD Kdf, byte[] PublicKeyData, byte[]? SharedData);
