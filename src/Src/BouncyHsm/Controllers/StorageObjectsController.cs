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
}
