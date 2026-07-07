namespace BouncyHsm.Core.UseCases.Contracts;

public record MigrationResult(int SucceededObjects, int FailedObjects);