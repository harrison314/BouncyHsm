namespace BouncyHsm.Core.Services.Contracts.Entities;

public record struct SupportedNameCurve(string Kind, string Name, string? NamedCurve, string Oid);
