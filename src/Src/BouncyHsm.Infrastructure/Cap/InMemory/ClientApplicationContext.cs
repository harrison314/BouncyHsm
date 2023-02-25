using BouncyHsm.Core.Services.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public class ClientApplicationContext : IClientApplicationContext
{
    private readonly ConcurrentDictionary<string, MemorySession> apps;

    public ClientApplicationContext()
    {
        this.apps = new ConcurrentDictionary<string, MemorySession>();
    }

    public IMemorySession RegisterMemorySession(string key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        return this.apps.AddOrUpdate(key,
            new MemorySession(),
            (_, _) => throw new BouncyHsmStorageException($"Application with key {key} already registred."));
    }

    public void ReleaseMemorySession(string key)
    {
        this.apps.Remove(key, out _);
    }

    public bool TryGetMemorySession(string key, [NotNullWhen(true)] out IMemorySession? memorySession)
    {
        bool result = this.apps.TryGetValue(key, out MemorySession? ms);
        memorySession = ms;
        return result;
    }
}
