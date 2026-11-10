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

    public bool IsContextSpecificLogged
    {
        get;
        set;
    }

    ISessionState State
    {
        get;
        set;
    }
}
