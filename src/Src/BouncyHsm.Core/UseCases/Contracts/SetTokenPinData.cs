using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public record SetTokenPinData(CKU UserType, string NewPin);