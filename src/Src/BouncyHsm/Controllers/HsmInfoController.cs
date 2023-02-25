using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.HsmInfo;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class HsmInfoController : Controller
{
    private readonly IHsmInfoFacade infoFacade;
    private readonly ILogger<HsmInfoController> logger;

    public HsmInfoController(IHsmInfoFacade infoFacade, ILogger<HsmInfoController> logger)
    {
        this.infoFacade = infoFacade;
        this.logger = logger;
    }

    [HttpGet("Versions", Name = nameof(GetVersions))]
    [ProducesResponseType(typeof(BouncyHsmVersionDto), 200)]
    public IActionResult GetVersions()
    {
        this.logger.LogTrace("Entering to GetVersions");

        return this.Ok(HsmInfoControllerMapper.ToDto(this.infoFacade.GetVersions()));
    }


    [HttpGet("SupportedCurves", Name = nameof(GetSupportedEcCurves))]
    [ProducesResponseType(typeof(IEnumerable<EcCurveInfoDto>), 200)]
    public IActionResult GetSupportedEcCurves()
    {
        this.logger.LogTrace("Entering to GetSupportedEcCurves");

        return this.Ok(HsmInfoControllerMapper.ToDto(this.infoFacade.GetCurves()));
    }

    [HttpGet("GetMechanism", Name = nameof(GetMechanism))]
    [ProducesResponseType(typeof(IEnumerable<MechanismInfoDto>), 200)]
    public IActionResult GetMechanism()
    {
        this.logger.LogTrace("Entering to GetMechanism");

        return this.Ok(HsmInfoControllerMapper.ToDto(this.infoFacade.GetAllMechanism()));
    }
}
