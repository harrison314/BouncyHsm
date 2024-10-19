using BouncyHsm.Core.Services.Contracts.Entities;
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

    DateTime StartAt
    {
        get;
    }

    DateTime LastActivity
    {
        get;
        set;
    }

    MemorySessionStatus GetStatus(uint? slotIdSpecification);

    uint CreateSession(uint slotId, bool isRwSession, SecureRandom secureRandom);

    bool DestroySession(uint sessionId);

    int DestroySessionsBySlot(uint slotId);

    bool TryGetSession(uint sessionId, [NotNullWhen(true)] out IP11Session? session);

    bool IsUserLogged(uint slotId);

    uint CreateHandle(StorageObject storageObject);

    Guid? FindObjectHandle(uint objectHandle);

    void DestroyObjectHandle(Guid id);

    uint? GetLastSlotEvent();
}
