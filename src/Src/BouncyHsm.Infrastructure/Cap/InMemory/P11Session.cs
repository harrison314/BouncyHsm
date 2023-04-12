using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public class P11Session : IP11Session
{
    private LoggedUser loggedUser;
    private List<StorageObjectMemento> objects;

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
        this.objects = new List<StorageObjectMemento>();
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
        return this.objects.Where(t => (specification.IsUserLogged || !t.GetCkaPrivate()) && t.IsMatch(specification.Template))
            .Select(t => StorageObjectFactory.CreateFromMemento(t))
            .ToList();
    }

    public void StoreObject(StorageObject storageObject)
    {
        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        this.objects.Add(storageObject.ToMemento());
    }

    public StorageObject? TryLoadObject(Guid id)
    {
        StorageObjectMemento? memnto = this.objects.FirstOrDefault(t => t.Id == id);
        return (memnto != null) ? StorageObjectFactory.CreateFromMemento(memnto) : null;
    }

    public void DestroyObject(StorageObject storageObject)
    {
        int count = this.objects.RemoveAll(t => t.Id == storageObject.Id);
        System.Diagnostics.Debug.Assert(count != 1);
    }
}
