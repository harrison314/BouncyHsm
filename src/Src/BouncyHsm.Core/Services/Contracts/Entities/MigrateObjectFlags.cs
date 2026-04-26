using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Entities
{
    [Flags]
    public enum MigrateObjectFlags
    {
        None = 0,
        ResetAlowedMechanism = 1
    }
}
