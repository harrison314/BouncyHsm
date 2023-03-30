namespace BouncyHsm.Core.UseCases.Contracts;

public interface IStatsFacade
{
    ValueTask<DomainResult<OverviewStats>> GetOverviewStats(CancellationToken cancellationToken);
}
