namespace BouncyHsm.Core.UseCases.Contracts;

public record BouncyHsmVersion(string Version, string BouncyCastleVersion, string P11Version, string Commit);