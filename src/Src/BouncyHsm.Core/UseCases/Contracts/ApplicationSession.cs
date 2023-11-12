namespace BouncyHsm.Core.UseCases.Contracts;

public record ApplicationSession(Guid ApplicationSessionId,
    string CompiuterName,
    string ApplicationName,
    uint Pid,
    DateTime StartAt,
    DateTime LastInteraction);