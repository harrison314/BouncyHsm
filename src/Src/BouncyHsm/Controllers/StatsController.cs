using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Stats;
using Microsoft.AspNetCore.Mvc;
using System.Security;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class StatsController : Controller
{
    private readonly IStatsFacade statsFacade;
    private readonly ILogger<StatsController> logger;

    public StatsController(IStatsFacade statsFacade, ILogger<StatsController> logger)
    {
        this.statsFacade = statsFacade;
        this.logger = logger;
    }

    [HttpGet(Name = nameof(GetOverviewStats))]
    [ProducesResponseType(typeof(OverviewStatsDto), 200)]
    public async Task<IActionResult> GetOverviewStats()
    {
        this.logger.LogTrace("Entering to GetAllSlots");

        DomainResult<OverviewStats> result = await this.statsFacade.GetOverviewStats(this.HttpContext.RequestAborted);

        return result.MapOk(StatsControllerMapper.ToDto).ToActionResult();
    }
}
