using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BouncyHsm.Infrastructure.HealthChecks;

internal class PersistentRepozitoryHealthCheck : IHealthCheck
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ILogger<PersistentRepozitoryHealthCheck> logger;

    public PersistentRepozitoryHealthCheck(IPersistentRepository persistentRepository, ILogger<PersistentRepozitoryHealthCheck> logger)
    {
        this.persistentRepository = persistentRepository;
        this.logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await this.persistentRepository.CheckHealth(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Problem in persistent repozitory.");
            return HealthCheckResult.Unhealthy("Persistent repozirory failed.", ex);
        }
    }
}
