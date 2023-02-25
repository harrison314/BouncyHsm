using BouncyHsm.Core.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class P11SessionExtensions
{
    public static void ClearState(this IP11Session session)
    {
        if (session.State is IDisposable disposable)
        {
            disposable.Dispose();
        }

        session.State = EmptySessionState.Instance;
    }
}
