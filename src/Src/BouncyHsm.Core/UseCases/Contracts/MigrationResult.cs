namespace BouncyHsm.Core.UseCases.Contracts;

public record MigrationResult(int SuccessedObjects, int FailedObjects);