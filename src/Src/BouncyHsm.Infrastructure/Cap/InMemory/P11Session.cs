using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

public class P11Session : IP11Session
{
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

    public bool IsContextSpecificLogged
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
        this.IsContextSpecificLogged = false;
    }

    public override string ToString()
    {
        return $"Session {this.SessionId} in slot {this.SlotId}";
    }
}
