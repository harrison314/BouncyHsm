using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public class SlotEntity : Entity
{
    public uint SlotId
    {
        get;
        set;
    }

    public bool IsHwDevice
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public required TokenInfo Token
    {
        get;
        set;
    }

    public SlotEntity()
    {
        this.Description = string.Empty;
    }
}
