namespace BouncyHsm.Core.Services.Contracts.Entities;

public record struct SupportedNameCurve(string Kind, string Name, string? NameInParameter, string Oid);
