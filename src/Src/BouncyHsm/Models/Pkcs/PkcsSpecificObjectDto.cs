using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Models.Pkcs;

public record PkcsSpecificObjectDto(CKO CkaClass, Guid ObjectId, string Description);