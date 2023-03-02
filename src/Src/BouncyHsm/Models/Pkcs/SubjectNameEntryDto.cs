namespace BouncyHsm.Models.Pkcs;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.SubjectNameEntry))]
public record SubjectNameEntryDto(string Oid, string Value);
