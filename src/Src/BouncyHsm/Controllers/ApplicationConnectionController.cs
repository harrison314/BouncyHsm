using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.ApplicationConnection;
using BouncyHsm.Models.Slot;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class ApplicationConnectionController : Controller
{
    private readonly IApplicationConnectionsFacade applicationConnectionsFacade;
    private readonly ILogger<ApplicationConnectionController> logger;

    public ApplicationConnectionController(IApplicationConnectionsFacade applicationConnectionsFacade,
        ILogger<ApplicationConnectionController> logger)
    {
        this.applicationConnectionsFacade = applicationConnectionsFacade;
        this.logger = logger;
    }

    [HttpGet(Name = nameof(GetApplicationConnections))]
    [ProducesResponseType(typeof(List<ApplicationSessionDto>), 200)]
    public async Task<IActionResult> GetApplicationConnections()
    {
        this.logger.LogTrace("Entering to GetApplicationConnections");

        DomainResult<SlotConnections> result = await this.applicationConnectionsFacade.GetApplicationConnections(this.HttpContext.RequestAborted);

        return result.MapOk(t => t.ApplicationSessions)
            .MapOk(ApplicationConnectionControllerMapper.ToDto)
            .ToActionResult();
    }

    [HttpDelete("{applicationSessionId}", Name = nameof(RemoveApplicationConnection))]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> RemoveApplicationConnection(Guid applicationSessionId)
    {
        this.logger.LogTrace("Entering to RemoveApplicationConnection with {applicationSessionId}.", applicationSessionId);

        VoidDomainResult result = await this.applicationConnectionsFacade.RemoveApplicationConnection(applicationSessionId, this.HttpContext.RequestAborted);

        return result.ToActionResult();
    }
}
