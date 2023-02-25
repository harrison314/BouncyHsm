using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public class P11Session : IP11Session
{
    private LoggedUser loggedUser;
    private List<StorageObject> objects;

    public uint SlotId
    {
        get;
        set;
    }

    public bool IsRwSession
    {
        get;
    }

    public uint SessionId
    {
        get;
        internal set;
    }

    public SecureRandom SecureRandom
    {
        get;
    }

    public ISessionState State
    {
        get;
        set;
    }

    public P11Session(uint slotId, bool isRwSession, SecureRandom secureRandom)
    {
        this.SlotId = slotId;
        this.IsRwSession = isRwSession;
        this.loggedUser = LoggedUser.None;
        this.SecureRandom = secureRandom;
        this.State = EmptySessionState.Instance;
        this.objects = new List<StorageObject>();
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

    public IReadOnlyList<StorageObject> FindObjects(FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        return this.objects.Where(t => (specification.IsUserLogged || !t.CkaPrivate) && t.IsMatch(specification.Template)).ToList();
    }

    public void StoreObject(StorageObject storageObject)
    {
        this.objects.Add(storageObject);
    }

    public StorageObject? TryLoadObject(Guid id)
    {
        return this.objects.FirstOrDefault(t => t.Id == id);
    }

    public void DestroyObject(StorageObject storageObject)
    {
        this.objects.RemoveAll(t => t.Id == storageObject.Id);
    }
}
