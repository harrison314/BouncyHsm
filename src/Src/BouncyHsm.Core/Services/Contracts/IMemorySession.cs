using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;
using System.Diagnostics.CodeAnalysis;

namespace BouncyHsm.Core.Services.Contracts;

public interface IMemorySession
{
    Guid Id
    {
        get;
    }

    MemorySessionData Data
    {
        get;
    }

    DateTimeOffset StartAt
    {
        get;
    }

    DateTimeOffset LastActivity
    {
        get;
        set;
    }

    MemorySessionStatus GetStatus(uint? slotIdSpecification);

    IP11Session CreateSession(uint slotId, bool isRwSession, SecureRandom secureRandom);

    bool DestroySession(uint sessionId);

    int DestroySessionsBySlot(uint slotId);

    bool TryGetSession(uint sessionId, [NotNullWhen(true)] out IP11Session? session);

    bool IsLogged(IP11Session session, CKU userType);

    void SetLoginStatus(IP11Session session, CKU userType, bool isLogged);

    uint CreateHandle(StorageObject storageObject);

    Guid? FindObjectHandle(uint objectHandle);

    void DestroyObjectHandle(Guid id);

    uint? GetLastSlotEvent();

    IReadOnlyList<StorageObject> FindObjects(IP11Session session, FindObjectSpecification specification, CancellationToken cancellationToken);

    StorageObject? TryLoadObject(IP11Session session, Guid id);

    void DestroyObject(IP11Session session, StorageObject storageObject);

    void StoreObject(IP11Session session, StorageObject storageObject);

    void UpdateObject(IP11Session session, StorageObject storageObject);
}
