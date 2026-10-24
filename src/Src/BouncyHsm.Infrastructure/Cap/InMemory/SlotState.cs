using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Infrastructure.Cap.InMemory;

internal class SlotState
{
    public HashSet<uint> Sessions
    {
        get;
        set;
    }

    public LoggedUser LoggedState
    {
        get;
        set;
    }

    public SlotState()
    {
        this.Sessions = new HashSet<uint>();
        this.LoggedState = LoggedUser.None;
    }
}
