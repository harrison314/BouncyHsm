using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public record PkcsSpecificObject(CKO CkaClass, Guid ObjectId, string Description);
