using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Migration;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class MigrationController : Controller
{
    private readonly IMigrationFacade migrationFacade;
    private readonly ILogger<MigrationController> logger;

    public MigrationController(IMigrationFacade migrationFacade, ILogger<MigrationController> logger)
    {
        this.migrationFacade = migrationFacade;
        this.logger = logger;
    }

    [HttpPost("", Name = nameof(Migrate))]
    [ProducesResponseType(typeof(MigrationResultDto), 200)]
    public async Task<IActionResult> Migrate([FromBody] MigrationRequestDto model)
    {
        this.logger.LogTrace("Entering to Migrate");

        MigrationRequest request = MigrationControllerMapper.MapFromDto(model);
        DomainResult<MigrationResult> domainResult = await this.migrationFacade.Migrate(request, this.HttpContext.RequestAborted);

        return domainResult.MapOk(MigrationControllerMapper.ToDto).ToActionResult();
    }
}