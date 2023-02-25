using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public class MemorySession : IMemorySession
{
    private readonly ConcurrentDictionary<uint, P11Session> sessions;
    private readonly ConcurrentDictionary<Guid, uint> objectHandlesToGuid;
    private readonly ConcurrentDictionary<uint, Guid> objectHandlesToHandles;

    public MemorySession()
    {
        this.sessions = new ConcurrentDictionary<uint, P11Session>();
        this.objectHandlesToGuid = new ConcurrentDictionary<Guid, uint>();
        this.objectHandlesToHandles = new ConcurrentDictionary<uint, Guid>();
    }

    public uint CreateSession(uint slotId, bool isRwSession, SecureRandom secureRandom)
    {
        P11Session newSession = new P11Session(slotId, isRwSession, secureRandom);

        for (uint sessionId = 1000; sessionId < 5_000_000; sessionId++)
        {
            if (this.sessions.TryAdd(sessionId, newSession))
            {
                newSession.SessionId = sessionId;
                return sessionId;
            }
        }

        throw new RpcPkcs11Exception(Core.Services.Contracts.P11.CKR.CKR_SESSION_COUNT, "All session is ");
    }

    public bool DestroySession(uint sessionId)
    {
        return this.sessions.Remove(sessionId, out _);
    }

    public int DestroySessionsBySlot(uint slotId)
    {
        List<uint> sessionsToRemove = new List<uint>();
        foreach ((uint sessionId, P11Session session) in this.sessions)
        {
            if (session.SlotId == slotId)
            {
                sessionsToRemove.Add(sessionId);
            }
        }

        foreach (uint sessionId in sessionsToRemove)
        {
            this.sessions.Remove(sessionId, out _);
        }

        return sessionsToRemove.Count;
    }

    public MemorySessionStatus GetStatus()
    {
        int rwSessions = 0;
        int roSessions = 0;

        foreach (P11Session session in this.sessions.Values)
        {
            if (session.IsRwSession)
            {
                rwSessions++;
            }
            else
            {
                roSessions++;
            }
        }


        return new MemorySessionStatus(roSessions, rwSessions);
    }

    public bool TryGetSession(uint sessionId, [NotNullWhen(true)] out IP11Session? session)
    {
        if (this.sessions.TryGetValue(sessionId, out P11Session? value))
        {
            session = value;
            return true;
        }
        else
        {
            session = null;
            return false;
        }
    }

    public bool IsUserLogged(uint slotId)
    {
        return this.sessions.Any(t => t.Value.SlotId == slotId && (t.Value.IsLogged(CKU.CKU_USER) || t.Value.IsLogged(CKU.CKU_SO)));
    }

    public uint CreateHandle(StorageObject storageObject)
    {
        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        if (this.objectHandlesToGuid.TryGetValue(storageObject.Id, out uint handle))
        {
            return handle;
        }

        for (uint handleId = 1000; handleId < 5_000_000; handleId++)
        {
            if (this.objectHandlesToHandles.TryAdd(handleId, storageObject.Id))
            {
                this.objectHandlesToGuid.TryAdd(storageObject.Id, handleId);
                return handleId;
            }
        }

        throw new Exception("Maximum handlers");
    }

    public Guid? FindObjectHandle(uint objectHandle)
    {
        if (this.objectHandlesToHandles.TryGetValue(objectHandle, out Guid id))
        {
            return id;
        }
        else
        {
            return null;
        }
    }

    public void DestroyObjectHandle(Guid id)
    {
        if (this.objectHandlesToGuid.TryRemove(id, out uint handle))
        {
            this.objectHandlesToHandles.TryRemove(handle, out _);
        }
    }
}
