using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Infrastructure.Common;

public class P11HwServices : IP11HwServices
{
    public ITimeAccessor Time
    {
        get;
    }

    public IPersistentRepository Persistence
    {
        get;
    }

    public IClientApplicationContext ClientAppCtx
    {
        get;
    }

    public P11HwServices(ITimeAccessor time, IPersistentRepository persistence, IClientApplicationContext clientAppCtx)
    {
        this.Time = time;
        this.Persistence = persistence;
        this.ClientAppCtx = clientAppCtx;
    }
}
