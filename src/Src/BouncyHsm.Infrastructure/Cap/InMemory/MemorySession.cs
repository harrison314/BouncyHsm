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
    private LoggedUser loggedUser;
    private readonly Lock slotEventsLock;
    private DateTimeOffset lastActivity;

    public Guid Id
    {
        get;
    }

    public MemorySessionData Data
    {
        get;
    }

    public DateTimeOffset StartAt
    {
        get;
    }

    public DateTimeOffset LastActivity
    {
        get => this.lastActivity;
        set
        {
            if (value > this.lastActivity)
            {
                this.lastActivity = value;
            }
        }
    }

    public MemorySession(MemorySessionData sessionData, DateTimeOffset startAt)
    {
        this.sessions = new ConcurrentDictionary<uint, P11Session>();
        this.objectHandlesToGuid = new ConcurrentDictionary<Guid, uint>();
        this.objectHandlesToHandles = new ConcurrentDictionary<uint, Guid>();
        this.slotEvents = new HashSet<uint>();
        this.Id = Guid.NewGuid();
        this.Data = sessionData;
        this.StartAt = startAt;
        this.lastActivity = startAt;
        this.slotEventsLock = new Lock();
        this.loggedUser = LoggedUser.None;
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

    public bool IsLogged(CKU userType)
    {
        return userType switch
        {
            CKU.CKU_SO => this.loggedUser.HasFlag(LoggedUser.So),
            CKU.CKU_USER => this.loggedUser.HasFlag(LoggedUser.User),
            CKU.CKU_CONTEXT_SPECIFIC => this.loggedUser.HasFlag(LoggedUser.ContextSpecific),
            _ => throw new InvalidProgramException($"Enum value {userType} is noz supported.")
        };
    }

    public void SetLoginStatus(CKU userType, bool isLogged)
    {
        LoggedUser userFlagType = userType switch
        {
            CKU.CKU_SO => LoggedUser.So,
            CKU.CKU_USER => LoggedUser.User,
            CKU.CKU_CONTEXT_SPECIFIC => LoggedUser.ContextSpecific,
            _ => throw new InvalidProgramException($"Enum value {userType} is noz supported.")
        };

        if (isLogged)
        {
            this.loggedUser |= userFlagType;
        }
        else
        {
            this.loggedUser &= ~userFlagType;
        }
    }

    public MemorySessionStatus GetStatus(uint? slotIdSpecification)
    {
        int rwSessions = 0;
        int roSessions = 0;

        if (slotIdSpecification.HasValue)
        {
            foreach (P11Session session in this.sessions.Values.Where(t => t.SlotId == slotIdSpecification.Value))
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
        }
        else
        {
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
        return this.loggedUser.HasFlag(LoggedUser.User) || this.loggedUser.HasFlag(LoggedUser.So);
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
        lock (this.slotEventsLock)
        {
            if (this.slotEvents.Count == 0)
            {
                return null;
            }

            uint slotId = this.slotEvents.First();
            this.slotEvents.Remove(slotId);

            return slotId;
        }
    }

    internal void NotifySlotEvent(uint slotId)
    {
        lock (this.slotEventsLock)
        {
            _ = this.slotEvents.Add(slotId);
        }
    }
}
