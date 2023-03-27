using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.UseCases.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Implementation;

public class StatsFacade : IStatsFacade
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<StatsFacade> logger;

    public StatsFacade(IP11HwServices hwServices, ILogger<StatsFacade> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DomainResult<OverviewStats>> GetOverviewStats(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetOverviewStats.");

        ClientApplicationContextStats appStats = this.hwServices.ClientAppCtx.GetStats();
        PersistentRepositoryStats storageStats = await this.hwServices.Persistence.GetStats(cancellationToken);

        OverviewStats stats = new OverviewStats()
        {
            ConnectedApplications = appStats.ConnectedApplications,
            RoSessionCount = appStats.RoSessionCount,
            RwSessionCount = appStats.RwSessionCount,
            SlotCount = storageStats.SlotCount,
            TotalObjectCount = storageStats.TotalObjectCount,
            PrivateKeys = storageStats.PrivateKeys,
            X509Certificates = storageStats.X509Certificates
        };

        return new DomainResult<OverviewStats>.Ok(stats);
    }
}
