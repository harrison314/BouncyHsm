using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Pkcs;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class PkcsController : Controller
{
    private readonly IPkcsFacade pkcsFacade;
    private readonly ILogger<PkcsController> logger;

    public PkcsController(IPkcsFacade pkcsFacade, ILogger<PkcsController> logger)
    {
        this.pkcsFacade = pkcsFacade;
        this.logger = logger;
    }

    [HttpPost("{slotId}/ImportP12", Name = nameof(ImportP12))]
    [ProducesResponseType(typeof(ImportP12ResponseDto), 200)]
    public async Task<IActionResult> ImportP12(uint slotId, [FromBody] ImportP12RequestDto model)
    {
        this.logger.LogTrace("Entering to ImportP12 with slotId {slotId}.", slotId);

        ImportP12Request request = PkcsControllerMapper.FromDto(model, slotId);
        DomainResult<Guid> privateKeyId = await this.pkcsFacade.ImportP12(request, this.HttpContext.RequestAborted);

        return privateKeyId.MapOk(t => new ImportP12ResponseDto() { PrivateKeyId = t }).ToActionResult();
    }
}
