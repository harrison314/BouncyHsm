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

    [HttpGet("{slotId}", Name = nameof(GetPkcsObjects))]
    [ProducesResponseType(typeof(ImportP12ResponseDto), 200)]
    public async Task<IActionResult> GetPkcsObjects(uint slotId)
    {
        this.logger.LogTrace("Entering to GetPkcsObjects with slotId {slotId}.", slotId);

        DomainResult<PkcsObjects> result = await this.pkcsFacade.GetObjects(slotId, this.HttpContext.RequestAborted);

        return result.MapOk(PkcsControllerMapper.ToDto).ToActionResult();
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

    [HttpPost("{slotId}/GeneratePkcs10")]
    [ProducesResponseType(typeof(Pkcs10Dto), 200)]
    public async Task<IActionResult> GetPkcsObjects(uint slotId, [FromBody] GeneratePkcs10RequestDto model)
    {
        this.logger.LogTrace("Entering to GetPkcsObjects with slotId {slotId}.", slotId);

        GeneratePkcs10Request request = PkcsControllerMapper.FromDto(model, slotId);
        DomainResult<byte[]> result = await this.pkcsFacade.GeneratePkcs10(request, this.HttpContext.RequestAborted);

        return result.MapOk(t => new Pkcs10Dto(t)).ToActionResult();
    }
}
