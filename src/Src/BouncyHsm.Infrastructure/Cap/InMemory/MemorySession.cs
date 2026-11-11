using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public sealed class MemorySession : IMemorySession, IDisposable
{
    private record struct StorageObjectData(uint OwnedSessionId, uint SlotId, StorageObjectMemento Memento);
    private readonly Dictionary<uint, P11Session> sessions;
    private readonly Dictionary<uint, SlotState> slotToSessions;
    private readonly Dictionary<Guid, uint> objectIdToHandles;
    private readonly Dictionary<uint, Guid> objectHandlesToObjectId;
    private readonly Dictionary<Guid, StorageObjectData> storageObjects;
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
        this.objectIdToHandles = new Dictionary<Guid, uint>();
        this.objectHandlesToObjectId = new Dictionary<uint, Guid>();
        this.storageObjects = new Dictionary<Guid, StorageObjectData>();
        this.slotEvents = new HashSet<uint>();
        this.Id = Guid.NewGuid();
        this.Data = sessionData;
        this.StartAt = startAt;
        this.lastActivity = startAt;
        this.slotEventsLock = new Lock();
        this.rwSessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        this.rwObjectLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
    }

    public IP11Session CreateSession(uint slotId, bool isRwSession, SecureRandom secureRandom)
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

                    return newSession;
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

                this.RemoveSessionObjects(sessionId);

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
                this.RemoveSessionObjects(sessionId);
            }

            this.slotToSessions.Remove(slotId);

            return sessionsToRemove.Count;
        }
        finally
        {
            this.rwSessionLock.ExitWriteLock();
        }
    }

    public bool IsLogged(IP11Session session, CKU userType)
    {
        this.rwSessionLock.EnterReadLock();
        try
        {
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

    public void SetLoginStatus(IP11Session session, CKU userType, bool isLogged)
    {
        this.rwSessionLock.EnterWriteLock();
        try
        {
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

    public uint CreateHandle(StorageObject storageObject)
    {
        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        this.rwObjectLock.EnterWriteLock();
        try
        {
            if (this.objectIdToHandles.TryGetValue(storageObject.Id, out uint handle))
            {
                return handle;
            }

            for (uint handleId = 1000; handleId < 5_000_000; handleId++)
            {
                if (this.objectHandlesToObjectId.TryAdd(handleId, storageObject.Id))
                {
                    this.objectIdToHandles.TryAdd(storageObject.Id, handleId);
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
            if (this.objectHandlesToObjectId.TryGetValue(objectHandle, out Guid id))
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

    public IReadOnlyList<StorageObject> FindObjects(IP11Session session, FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        this.rwObjectLock.EnterReadLock();
        try
        {
            return this.storageObjects.Values.Where(t => t.SlotId == session.SlotId
            && (specification.IsUserLogged || !t.Memento.GetCkaPrivate())
            && t.Memento.IsMatch(specification.Template))
                 .Select(t => StorageObjectFactory.CreateFromMemento(t.Memento))
                 .ToList();
        }
        finally
        {
            this.rwObjectLock.ExitReadLock();
        }
    }

    public StorageObject? TryLoadObject(IP11Session session, Guid id)
    {
        this.rwObjectLock.EnterReadLock();
        try
        {
            if (this.storageObjects.TryGetValue(id, out StorageObjectData data))
            {
                if (data.SlotId != session.SlotId)
                {
                    throw new ArgumentException($"The session with ID {session.SessionId} ​​does not belong to the slot in which the session object {data.Memento.Id} was created.");
                }

                return StorageObjectFactory.CreateFromMemento(data.Memento);
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

    public void DestroyObject(IP11Session session, StorageObject storageObject)
    {
        this.rwObjectLock.EnterWriteLock();
        try
        {
            if (this.storageObjects.TryGetValue(storageObject.Id, out StorageObjectData data))
            {
                if (data.SlotId != session.SlotId)
                {
                    throw new ArgumentException($"The session with ID {session.SessionId} ​​does not belong to the slot in which the session object {storageObject.Id} was created.");
                }

                this.storageObjects.Remove(storageObject.Id);
            }
            else
            {
                System.Diagnostics.Debug.Fail("Object not exists.");
            }
        }
        finally
        {
            this.rwObjectLock.ExitWriteLock();
        }
    }

    public void StoreObject(IP11Session session, StorageObject storageObject)
    {
        this.rwObjectLock.EnterWriteLock();
        try
        {
            if (storageObject.Id == Guid.Empty)
            {
                storageObject.Id = Guid.CreateVersion7();
            }

            this.storageObjects.Add(storageObject.Id,
                new StorageObjectData(session.SessionId, session.SlotId, storageObject.ToMemento()));
        }
        finally
        {
            this.rwObjectLock.ExitWriteLock();
        }
    }

    public void UpdateObject(IP11Session session, StorageObject storageObject)
    {
        this.rwObjectLock.EnterWriteLock();
        try
        {
            if (this.storageObjects.TryGetValue(storageObject.Id, out StorageObjectData data))
            {
                if (session.SlotId != data.SlotId)
                {
                    throw new ArgumentException($"The session with ID {session.SessionId} ​​does not belong to the slot in which the session object {storageObject.Id} was created.");
                }

                this.storageObjects[storageObject.Id] = new StorageObjectData(data.OwnedSessionId,
                    data.SlotId,
                    storageObject.ToMemento());
            }
            else
            {
                System.Diagnostics.Debug.Fail("Storage object does not exists.");
            }
        }
        finally
        {
            this.rwObjectLock.ExitWriteLock();
        }
    }

    public void DestroyObjectHandle(Guid id)
    {
        this.rwObjectLock.EnterWriteLock();
        try
        {
            if (this.objectIdToHandles.Remove(id, out uint handle))
            {
                this.objectHandlesToObjectId.Remove(handle, out _);
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

    private void RemoveSessionObjects(uint sessionId)
    {
        List<Guid> sessionObjects = this.storageObjects.Where(t => t.Value.OwnedSessionId == sessionId)
            .Select(t => t.Key)
            .ToList();

        foreach (Guid objectId in sessionObjects)
        {
            this.storageObjects.Remove(objectId);
            if (this.objectIdToHandles.Remove(objectId, out uint handleId))
            {
                this.objectHandlesToObjectId.Remove(handleId);
            }
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
