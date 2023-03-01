
namespace BouncyHsm.Models.HsmInfo;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.BouncyHsmVersion))]
public record BouncyHsmVersionDto(string Version, string BouncyCastleVersion, string P11Version, string Commit);