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
    private readonly ITimeAccessor timeAccessor;

    public ClientApplicationContext(ITimeAccessor timeAccessor)
    {
        this.apps = new ConcurrentDictionary<string, MemorySession>();
        this.timeAccessor = timeAccessor;
    }

    public IMemorySession RegisterMemorySession(string key, MemorySessionData sessionData)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (sessionData == null) throw new ArgumentNullException(nameof(sessionData));

        return this.apps.AddOrUpdate(key,
            new MemorySession(sessionData, this.timeAccessor.UtcNow),
            (_, _) => throw new RpcPkcs11Exception(Core.Services.Contracts.P11.CKR.CKR_CRYPTOKI_ALREADY_INITIALIZED, $"Application with key {key} already registred."));
    }

    public void ReleaseMemorySession(string key)
    {
        this.apps.Remove(key, out _);
    }

    public void ReleaseMemorySession(Guid applicationSessionId)
    {
        string? selectedKey = null;
        foreach ((string key, IMemorySession session) in this.apps)
        {
            if (session.Id == applicationSessionId)
            {
                selectedKey = key;
                break;
            }
        }

        if (selectedKey == null)
        {
            throw new BouncyHsmInvalidInputException($"Not found memory session with id {applicationSessionId}.");
        }

        this.apps.Remove(selectedKey, out _);
    }

    public bool TryGetMemorySession(string key, [NotNullWhen(true)] out IMemorySession? memorySession)
    {
        if (this.apps.TryGetValue(key, out MemorySession? ms))
        {
            ms.LastActivity = this.timeAccessor.UtcNow;
            memorySession = ms;
            return true;
        }
        else
        {
            memorySession = null;
            return false;
        }
    }

    public ClientApplicationContextStats GetStats()
    {
        int totalCount = 0;
        int rwSessionCount = 0;
        int roSessionCount = 0;

        foreach (MemorySession ms in this.apps.Values)
        {
            MemorySessionStatus status = ms.GetStatus(null);
            rwSessionCount += status.RwSessionCount;
            roSessionCount += status.RoSessionCount;
            totalCount++;
        }

        return new ClientApplicationContextStats(totalCount,
            roSessionCount,
            rwSessionCount);
    }

    public IEnumerable<IMemorySession> GetActiveMemorySessions()
    {
        return this.apps.Values;
    }

    public void NotifySlotEvent(uint slotId)
    {
        foreach (MemorySession ms in this.apps.Values)
        {
            ms.NotifySlotEvent(slotId);
        }
    }
}
