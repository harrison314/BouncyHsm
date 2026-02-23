using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.Contracts.Encapsulators;

internal record EncapsulationResult(SecretKeyObject KeyObject, byte[] EncapsulatedData);
