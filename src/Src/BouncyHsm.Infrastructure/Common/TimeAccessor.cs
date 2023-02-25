using BouncyHsm.Core.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Common;

public class TimeAccessor : ITimeAccessor
{
    public DateTime UtcNow
    {
        get => DateTime.UtcNow;
    }

    public TimeAccessor()
    {

    }
}
