namespace BouncyHsm.Core.UseCases.Contracts;

public record ApplicationSession(Guid ApplicationSessionId,
    string ComputerName,
    string ApplicationName,
    uint Pid,
    DateTime StartAt,
    DateTime LastInteraction);