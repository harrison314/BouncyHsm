using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Core.Services.Contracts;

public interface IP11HwServices
{
    ITimeAccessor Time
    {
        get;
    }

    IPersistentRepository Persistence
    {
        get;
    }

    IClientApplicationContext ClientAppCtx
    {
        get;
    }
}