using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public sealed class MemorySession : IMemorySession, IDisposable
{
    private readonly Dictionary<uint, P11Session> sessions;
    private readonly Dictionary<uint, SlotState> slotToSessions;
    private readonly Dictionary<Guid, uint> objectHandlesToGuid;
    private readonly Dictionary<uint, Guid> objectHandlesToHandles;
    private readonly HashSet<uint> slotEvents;
    private readonly Lock slotEventsLock;
    private readonly ReaderWriterLockSlim rwSessionLock;
    private readonly ReaderWriterLockSlim rwObjectLock;
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
        this.sessions = new Dictionary<uint, P11Session>();
        this.slotToSessions = new Dictionary<uint, SlotState>();
        this.objectHandlesToGuid = new Dictionary<Guid, uint>();
        this.objectHandlesToHandles = new Dictionary<uint, Guid>();
        this.slotEvents = new HashSet<uint>();
        this.Id = Guid.NewGuid();
        this.Data = sessionData;
        this.StartAt = startAt;
        this.lastActivity = startAt;
        this.slotEventsLock = new Lock();
        this.rwSessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        this.rwObjectLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    public uint CreateSession(uint slotId, bool isRwSession, SecureRandom secureRandom)
    {
        P11Session newSession = new P11Session(slotId, isRwSession, secureRandom);

        this.rwSessionLock.EnterWriteLock();
        try
        {
            for (uint sessionId = 1000; sessionId < 5_000_000; sessionId++)
            {
                if (this.sessions.TryAdd(sessionId, newSession))
                {
                    newSession.SessionId = sessionId;

                    if (this.slotToSessions.TryGetValue(slotId, out SlotState? slotState))
                    {
                        slotState.Sessions.Add(sessionId);
                    }
                    else
                    {
                        SlotState newSlotState = new SlotState();
                        newSlotState.Sessions.Add(sessionId);
                        this.slotToSessions.Add(slotId, newSlotState);
                    }

                    return sessionId;
                }
            }

            throw new RpcPkcs11Exception(CKR.CKR_SESSION_COUNT, "The maximum number of sessions has been reached.");
        }
        finally
        {
            this.rwSessionLock.ExitWriteLock();
        }
    }

    public bool DestroySession(uint sessionId)
    {
        this.rwSessionLock.EnterWriteLock();
        try
        {
            if (this.sessions.Remove(sessionId, out P11Session? p11Session))
            {
                if (this.slotToSessions.TryGetValue(p11Session.SlotId, out SlotState? slotState))
                {
                    slotState.Sessions.Remove(sessionId);
                    if (slotState.Sessions.Count == 0)
                    {
                        this.slotToSessions.Remove(p11Session.SlotId);
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        finally
        {
            this.rwSessionLock.ExitWriteLock();
        }
    }

    public int DestroySessionsBySlot(uint slotId)
    {
        this.rwSessionLock.EnterWriteLock();
        try
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

            this.slotToSessions.Remove(slotId);

            return sessionsToRemove.Count;
        }
        finally
        {
            this.rwSessionLock.ExitWriteLock();
        }
    }

    public bool IsLogged(uint sessionId, CKU userType)
    {
        this.rwSessionLock.EnterReadLock();
        try
        {
            if (!this.sessions.TryGetValue(sessionId, out P11Session? session))
            {
                throw new RpcPkcs11Exception(CKR.CKR_SESSION_HANDLE_INVALID, $"Session {sessionId} does not exist.");
            }

            LoggedUser loggedUser = this.slotToSessions[session.SlotId].LoggedState;
            return userType switch
            {
                CKU.CKU_SO => loggedUser.HasFlag(LoggedUser.So),
                CKU.CKU_USER => loggedUser.HasFlag(LoggedUser.User),
                CKU.CKU_CONTEXT_SPECIFIC => session.IsContextSpecificLogged,
                _ => throw new InvalidProgramException($"Enum value {userType} is not supported.")
            };
        }
        finally
        {
            this.rwSessionLock.ExitReadLock();
        }
    }

    public void SetLoginStatus(uint sessionId, CKU userType, bool isLogged)
    {
        this.rwSessionLock.EnterWriteLock();
        try
        {
            if (!this.sessions.TryGetValue(sessionId, out P11Session? session))
            {
                throw new RpcPkcs11Exception(CKR.CKR_SESSION_HANDLE_INVALID, $"Session {sessionId} does not exist.");
            }

            if (userType == CKU.CKU_CONTEXT_SPECIFIC)
            {
                session.IsContextSpecificLogged = isLogged;

                return;
            }

            LoggedUser userFlagType = userType switch
            {
                CKU.CKU_SO => LoggedUser.So,
                CKU.CKU_USER => LoggedUser.User,
                _ => throw new InvalidProgramException($"Enum value {userType} is not supported.")
            };

            if (isLogged)
            {
                this.slotToSessions[session.SlotId].LoggedState |= userFlagType;
            }
            else
            {
                this.slotToSessions[session.SlotId].LoggedState &= ~userFlagType;
            }
        }
        finally
        {
            this.rwSessionLock.ExitWriteLock();
        }
    }

    public MemorySessionStatus GetStatus(uint? slotIdSpecification)
    {
        int rwSessions = 0;
        int roSessions = 0;

        this.rwSessionLock.EnterReadLock();
        try
        {


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
        finally
        {
            this.rwSessionLock.ExitReadLock();
        }
    }

    public bool TryGetSession(uint sessionId, [NotNullWhen(true)] out IP11Session? session)
    {
        this.rwSessionLock.EnterReadLock();
        try
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
        finally
        {
            this.rwSessionLock.ExitReadLock();
        }
    }

    public bool IsUserLogged(uint slotId)
    {
        this.rwSessionLock.EnterReadLock();
        try
        {
            if (!this.slotToSessions.TryGetValue(slotId, out SlotState? slotState))
            {
                throw new RpcPkcs11Exception(CKR.CKR_SLOT_ID_INVALID, $"Slot {slotId} does not exist.");
            }

            return slotState.LoggedState.HasFlag(LoggedUser.User) || slotState.LoggedState.HasFlag(LoggedUser.So);
        }
        finally
        {
            this.rwSessionLock.ExitReadLock();
        }
    }

    public uint CreateHandle(StorageObject storageObject)
    {
        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        this.rwObjectLock.EnterWriteLock();
        try
        {
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
        finally
        {
            this.rwObjectLock.ExitWriteLock();
        }
    }

    public Guid? FindObjectHandle(uint objectHandle)
    {
        this.rwObjectLock.EnterReadLock();
        try
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
        finally
        {
            this.rwObjectLock.ExitReadLock();
        }
    }

    public void DestroyObjectHandle(Guid id)
    {
        this.rwObjectLock.EnterWriteLock();
        try
        {
            if (this.objectHandlesToGuid.Remove(id, out uint handle))
            {
                this.objectHandlesToHandles.Remove(handle, out _);
            }
        }
        finally
        {
            this.rwObjectLock.ExitWriteLock();
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

    public void Dispose()
    {
        this.rwSessionLock.Dispose();
        this.rwObjectLock.Dispose();
    }

    internal void NotifySlotEvent(uint slotId)
    {
        lock (this.slotEventsLock)
        {
            _ = this.slotEvents.Add(slotId);
        }
    }
}
