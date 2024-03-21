namespace BouncyHsm.Core.UseCases.Contracts;

public record MechanismProfile(string? ProfileName, List<MechanismInfoData> Mechanisms);