using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.PkcsSpecificObject))]
public record PkcsSpecificObjectDto(CKO CkaClass, Guid ObjectId, string Description);
