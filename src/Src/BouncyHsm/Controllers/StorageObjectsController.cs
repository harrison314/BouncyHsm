using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.HsmInfo;
using BouncyHsm.Models.StorageObjects;
using Microsoft.AspNetCore.Mvc;
using BouncyHsm.Core.Infrastructure.Extensions;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class StorageObjectsController : Controller
{
    private readonly IStorageObjectsFacade storageObjectsFacade;
    private readonly ILogger<StorageObjectsController> logger;

    public StorageObjectsController(IStorageObjectsFacade storageObjectsFacade, ILogger<StorageObjectsController> logger)
    {
        this.storageObjectsFacade = storageObjectsFacade;
        this.logger = logger;
    }

    [HttpGet("{slotId}", Name = nameof(GetStorageObjects))]
    [ProducesResponseType(typeof(StorageObjectsListDto), 200)]
    public async Task<IActionResult> GetStorageObjects(uint slotId, int skip = 0, int? take = null)
    {
        this.logger.LogTrace("Entering to GetStorageObjects");

        DomainResult<StorageObjectsList> result = await this.storageObjectsFacade.GetStorageObjects(slotId, skip, take ?? int.MaxValue, this.HttpContext.RequestAborted);

        return result.MapOk(StorageObjectsControllerMapper.ToDto).ToActionResult();
    }

    [HttpGet("{slotId}/{objectId}", Name = nameof(GetStorageObject))]
    [ProducesResponseType(typeof(StorageObjectDetailDto), 200)]
    public async Task<IActionResult> GetStorageObject(uint slotId, Guid objectId)
    {
        this.logger.LogTrace("Entering to GetStorageObject with slotId {slotId}, objectId {objectId}.", slotId, objectId);

        DomainResult<StorageObjectDetail> result = await this.storageObjectsFacade.GetStorageObject(slotId, objectId, this.HttpContext.RequestAborted);

        return result.MapOk(StorageObjectsControllerMapper.ToDto).ToActionResult();
    }

    [HttpDelete("{slotId}/{objectId}", Name = nameof(RemoveStorageObject))]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> RemoveStorageObject(uint slotId, Guid objectId)
    {
        this.logger.LogTrace("Entering to GetStorageObject with slotId {slotId}, objectId {objectId}.", slotId, objectId);

        VoidDomainResult result = await this.storageObjectsFacade.DeleteStorageObject(slotId, objectId, this.HttpContext.RequestAborted);

        return result.ToActionResult();
    }

    [HttpGet("{slotId}/{objectId}/Content", Name = nameof(GetObjectContent))]
    [ProducesResponseType(typeof(ObjectContentDto), 200)]
    public async Task<IActionResult> GetObjectContent(uint slotId, Guid objectId)
    {
        this.logger.LogTrace("Entering to GetObjectContent with slotId {slotId}, objectId {objectId}.", slotId, objectId);

        DomainResult<ObjectContent> result = await this.storageObjectsFacade.Download(slotId, objectId, this.HttpContext.RequestAborted);

        return result.MapOk(StorageObjectsControllerMapper.ToDto).ToActionResult();
    }

    [HttpGet("{slotId}/{objectId}/Attribute/{attributeName}", Name = nameof(GetAttribute))]
    [ProducesResponseType(typeof(HighLevelAttributeValueDto), 200)]
    public async Task<IActionResult> GetAttribute(uint slotId, Guid objectId, string attributeName)
    {
        this.logger.LogTrace("Entering to GetAttribute with slotId {slotId}, objectId {objectId}, attributeName {attributeName}.",
            slotId,
            objectId,
            attributeName);

        DomainResult<HighLevelAttributeValue> result = await this.storageObjectsFacade.GetObjectAttribute(slotId, objectId, attributeName, this.HttpContext.RequestAborted);

        return result.MapOk(StorageObjectsControllerMapper.ToDto).ToActionResult();
    }

    [HttpPost("{slotId}/{objectId}/Attribute/{attributeName}", Name = nameof(SetAttribute))]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<IActionResult> SetAttribute(uint slotId, Guid objectId, string attributeName, [FromBody] HighLevelAttributeValueDto model)
    {
        this.logger.LogTrace("Entering to SetAttribute with slotId {slotId}, objectId {objectId}, attributeName {attributeName}.",
            slotId,
            objectId,
            attributeName);

        HighLevelAttributeValue attributeValue = StorageObjectsControllerMapper.FromDto(model);
        await this.storageObjectsFacade.SetObjectAttribute(slotId, objectId, attributeName, attributeValue, this.HttpContext.RequestAborted);

        return this.Ok();
    }
}
