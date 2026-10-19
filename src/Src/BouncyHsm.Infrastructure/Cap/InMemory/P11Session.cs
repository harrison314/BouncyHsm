using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public class P11Session : IP11Session
{
  
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
        
        this.SecureRandom = secureRandom;
        this.State = EmptySessionState.Instance;
        this.objects = new List<StorageObjectMemento>();
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
            storageObject.Id = Guid.CreateVersion7();
        }

        this.objects.Add(storageObject.ToMemento());
    }

    public void UpdateObject(StorageObject storageObject)
    {
        int count = this.objects.RemoveAll(t => t.Id == storageObject.Id);
        System.Diagnostics.Debug.Assert(count == 1);
        this.objects.Add(storageObject.ToMemento());
    }

    public StorageObject? TryLoadObject(Guid id)
    {
        StorageObjectMemento? memento = this.objects.FirstOrDefault(t => t.Id == id);
        return (memento != null) ? StorageObjectFactory.CreateFromMemento(memento) : null;
    }

    public void DestroyObject(StorageObject storageObject)
    {
        int count = this.objects.RemoveAll(t => t.Id == storageObject.Id);
        System.Diagnostics.Debug.Assert(count == 1);
    }
}
