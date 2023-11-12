using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.Contracts;

public interface IClientApplicationContext
{
    IMemorySession RegisterMemorySession(string key, MemorySessionData sessionData);

    void ReleaseMemorySession(string key);

    void ReleaseMemorySession(Guid applicationSessionId);

    bool TryGetMemorySession(string key, [NotNullWhen(true)] out IMemorySession? memorySession);

    ClientApplicationContextStats GetStats();

    IEnumerable<IMemorySession> GetActiveMemorySessions();
}
