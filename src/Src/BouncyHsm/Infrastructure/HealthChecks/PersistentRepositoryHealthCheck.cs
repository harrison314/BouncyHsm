using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BouncyHsm.Infrastructure.HealthChecks;

internal class PersistentRepositoryHealthCheck : IHealthCheck
{
    private readonly IPersistentRepository persistentRepository;
    private readonly ILogger<PersistentRepositoryHealthCheck> logger;

    public PersistentRepositoryHealthCheck(IPersistentRepository persistentRepository, ILogger<PersistentRepositoryHealthCheck> logger)
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
            this.logger.LogError(ex, "Problem in persistent repository.");
            return HealthCheckResult.Unhealthy("Persistent reposirory failed.", ex);
        }
    }
}
