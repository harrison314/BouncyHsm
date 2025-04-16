using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.KeyGeneration;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class KeyGenerationController : Controller
{
    private readonly IKeysGenerationFacade keyGenerationFacade;
    private readonly ILogger<KeyGenerationController> logger;

    public KeyGenerationController(IKeysGenerationFacade keyGenerationFacade, ILogger<KeyGenerationController> logger)
    {
        this.keyGenerationFacade = keyGenerationFacade;
        this.logger = logger;
    }

    [HttpPost("{slotId}/GenerateRsaKeyPair", Name = nameof(GenerateRsaKeyPair))]
    [ProducesResponseType(typeof(GeneratedKeyPairIdsDto), 200)]
    public async Task<IActionResult> GenerateRsaKeyPair(uint slotId, [FromBody] GenerateRsaKeyPairRequestDto model)
    {
        this.logger.LogTrace("Entering to GenerateRsaKeyPair with slotId {slotId}.", slotId);

        GenerateRsaKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
        DomainResult<GeneratedKeyPairIds> result = await this.keyGenerationFacade.GenerateRsaKeyPair(slotId, request, this.HttpContext.RequestAborted);

        return result.MapOk(KeyGenerationControllerMapper.ToDto).ToActionResult();
    }

    [HttpPost("{slotId}/GenerateEcKeyPair", Name = nameof(GenerateEcKeyPair))]
    [ProducesResponseType(typeof(GeneratedKeyPairIdsDto), 200)]
    public async Task<IActionResult> GenerateEcKeyPair(uint slotId, [FromBody] GenerateEcKeyPairRequestDto model)
    {
        this.logger.LogTrace("Entering to GenerateEcKeyPair with slotId {slotId}.", slotId);

        GenerateEcKeyPairRequest request = KeyGenerationControllerMapper.MapFromDto(model);
        DomainResult<GeneratedKeyPairIds> result = await this.keyGenerationFacade.GenerateEcKeyPair(slotId, request, this.HttpContext.RequestAborted);

        return result.MapOk(KeyGenerationControllerMapper.ToDto).ToActionResult();
    }

    [HttpPost("{slotId}/GenerateAesKey", Name = nameof(GenerateAesKey))]
    [ProducesResponseType(typeof(GeneratedKeyPairIdsDto), 200)]
    public async Task<IActionResult> GenerateAesKey(uint slotId, [FromBody] GenerateAesKeyRequestDto model)
    {
        this.logger.LogTrace("Entering to GenerateEcKeyPair with slotId {slotId}.", slotId);

        GenerateAesKeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
        DomainResult<GeneratedSecretId> result = await this.keyGenerationFacade.GenerateAesKey(slotId, request, this.HttpContext.RequestAborted);

        return result.MapOk(KeyGenerationControllerMapper.ToDto).ToActionResult();
    }

    [HttpPost("{slotId}/GeneratePoly1305Key", Name = nameof(GeneratePoly1305Key))]
    [ProducesResponseType(typeof(GeneratedKeyPairIdsDto), 200)]
    public async Task<IActionResult> GeneratePoly1305Key(uint slotId, [FromBody] GeneratePoly1305KeyRequestDto model)
    {
        this.logger.LogTrace("Entering to GenerateEcKeyPair with slotId {slotId}.", slotId);

        GeneratePoly1305KeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
        DomainResult<GeneratedSecretId> result = await this.keyGenerationFacade.GeneratePoly1305Key(slotId, request, this.HttpContext.RequestAborted);

        return result.MapOk(KeyGenerationControllerMapper.ToDto).ToActionResult();
    }

    [HttpPost("{slotId}/GenerateSecretKey", Name = nameof(GenerateSecretKey))]
    [ProducesResponseType(typeof(GeneratedKeyPairIdsDto), 200)]
    public async Task<IActionResult> GenerateSecretKey(uint slotId, [FromBody] GenerateSecretKeyRequestDto model)
    {
        this.logger.LogTrace("Entering to GenerateEcKeyPair with slotId {slotId}.", slotId);

        GenerateSecretKeyRequest request = KeyGenerationControllerMapper.MapFromDto(model);
        DomainResult<GeneratedSecretId> result = await this.keyGenerationFacade.GenerateSecretKey(slotId, request, this.HttpContext.RequestAborted);

        return result.MapOk(KeyGenerationControllerMapper.ToDto).ToActionResult();
    }
}
