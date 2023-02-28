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

    [HttpPost("{slotId}/GeneratePkcs10")]
    [ProducesResponseType(typeof(Pkcs10Dto), 200)]
    public async Task<IActionResult> GetPkcsObjects(uint slotId, [FromBody] GeneratePkcs10RequestDto model)
    {
        this.logger.LogTrace("Entering to GetPkcsObjects with slotId {slotId}.", slotId);

        GeneratePkcs10Request request = PkcsControllerMapper.FromDto(model, slotId);
        DomainResult<byte[]> result = await this.pkcsFacade.GeneratePkcs10(request, this.HttpContext.RequestAborted);

        return result.MapOk(t => new Pkcs10Dto(t)).ToActionResult();
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

    [HttpPost("{slotId}/ImportX509Certificate", Name = nameof(ImportX509Certificate))]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> ImportX509Certificate(uint slotId, [FromBody] ImportX509CertificateRequestDto model)
    {
        this.logger.LogTrace("Entering to ImportX509Certificate with slotId {slotId}.", slotId);

        ImportX509CertificateRequest request = PkcsControllerMapper.FromDto(model, slotId);
        DomainResult<Guid> result = await this.pkcsFacade.ImportX509Certificate(request, this.HttpContext.RequestAborted);

        return result.MapOkToVoid().ToActionResult();
    }

    [HttpDelete("{slotId}/AssociatedObjects/{objectId}", Name = nameof(DeleteAssociatedObject))]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> DeleteAssociatedObject(uint slotId, Guid objectId)
    {
        this.logger.LogTrace("Entering to DeleteAssociatedObject with slotId {slotId}, objectId {objectId}.", slotId, objectId);

        VoidDomainResult result = await this.pkcsFacade.DeteleAssociatedObjects(slotId, objectId, this.HttpContext.RequestAborted);

        return result.ToActionResult();
    }
}
