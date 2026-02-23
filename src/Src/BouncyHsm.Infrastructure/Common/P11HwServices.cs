using BouncyHsm.Core.Services.Contracts;

namespace BouncyHsm.Infrastructure.Common;

public class P11HwServices : IP11HwServices
{
    public TimeProvider Time
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

    public P11HwServices(TimeProvider timeProvider, IPersistentRepository persistence, IClientApplicationContext clientAppCtx)
    {
        this.Time = timeProvider;
        this.Persistence = persistence;
        this.ClientAppCtx = clientAppCtx;
    }
}
