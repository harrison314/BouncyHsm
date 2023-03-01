namespace BouncyHsm.Models.StorageObjects;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.ObjectContent))]
public record ObjectContentDto(string FileName, byte[] Content);