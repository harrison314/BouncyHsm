using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.Contracts;

public interface IClientApplicationContext
{
    IMemorySession RegisterMemorySession(string key);

    void ReleaseMemorySession(string key);

    bool TryGetMemorySession(string key, [NotNullWhen(true)] out IMemorySession? memorySession);

    ClientApplicationContextStats GetStats();
}
