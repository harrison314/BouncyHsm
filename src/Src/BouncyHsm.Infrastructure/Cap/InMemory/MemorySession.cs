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
    private readonly HashSet<uint> slotEvents;
    private DateTime lastActivity;

    public Guid Id
    {
        get;
    }

    public MemorySessionData Data
    {
        get;
    }

    public DateTime StartAt
    {
        get;
    }

    public DateTime LastActivity
    {
        get => this.lastActivity;
        set => this.lastActivity = (value >= this.lastActivity)
            ? value
            : throw new ArgumentException("The last activity must be later than the start and the last activity.", nameof(this.LastActivity));
    }

    public MemorySession(MemorySessionData sessionData, DateTime startAt)
    {
        this.sessions = new ConcurrentDictionary<uint, P11Session>();
        this.objectHandlesToGuid = new ConcurrentDictionary<Guid, uint>();
        this.objectHandlesToHandles = new ConcurrentDictionary<uint, Guid>();
        this.slotEvents = new HashSet<uint>();
        this.Id = Guid.NewGuid();
        this.Data = sessionData;
        this.StartAt = startAt;
        this.lastActivity = startAt;
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

        throw new RpcPkcs11Exception(CKR.CKR_SESSION_COUNT, "The maximum number of sessions has been reached.");
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

        throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "The maximum number of object handles has been reached.");
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

    public uint? GetLastSlotEvent()
    {
        lock (this.slotEvents)
        {
            uint? slotId = this.slotEvents.FirstOrDefault();
            if (slotId.HasValue)
            {
                this.slotEvents.Remove(slotId.Value);
            }

            return slotId;
        }
    }

    internal void NotifySlotEvent(uint slotId)
    {
        lock (this.slotEvents)
        {
            _ = this.slotEvents.Add(slotId);
        }
    }
}
