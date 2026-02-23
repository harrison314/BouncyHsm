namespace BouncyHsm.Core.UseCases.Contracts;

public record ApplicationSession(Guid ApplicationSessionId,
    string ComputerName,
    string ApplicationName,
    uint Pid,
    string[] CmdArguments,
    DateTimeOffset StartAt,
    DateTimeOffset LastInteraction);