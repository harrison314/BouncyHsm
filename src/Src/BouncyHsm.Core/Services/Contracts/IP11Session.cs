using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts;

public interface IP11Session
{
    uint SessionId
    {
        get;
    }

    uint SlotId
    {
        get;
    }

    bool IsRwSession
    {
        get;
    }

    SecureRandom SecureRandom
    {
        get;
    }

    ISessionState State
    {
        get;
        set;
    }

    IReadOnlyList<StorageObject> FindObjects(FindObjectSpecification specification, CancellationToken cancellationToken);

    void StoreObject(StorageObject storageObject);

    void UpdateObject(StorageObject storageObject);

    StorageObject? TryLoadObject(Guid id);

    void DestroyObject(StorageObject storageObject);
}
