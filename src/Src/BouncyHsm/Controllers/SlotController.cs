using BouncyHsm.Core.Infrastructure.Extensions;
using BouncyHsm.Core.UseCases.Contracts;
using BouncyHsm.Models.Slot;
using Microsoft.AspNetCore.Mvc;

namespace BouncyHsm.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ProblemDetails), 400), ProducesResponseType(typeof(ProblemDetails), 500)]
public class SlotController : Controller
{
    private readonly ISlotFacade slotFacade;
    private readonly ILogger<SlotController> logger;

    public SlotController(ISlotFacade slotFacade, ILogger<SlotController> logger)
    {
        this.slotFacade = slotFacade;
        this.logger = logger;
    }

    [HttpPost(Name = nameof(CreateSlot))]
    [ProducesResponseType(typeof(CreateSlotResultDto), 200)]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotDto createSlotDto)
    {
        this.logger.LogTrace("Entering to CreateSlot");

        CreateSlotData createRequest = SlotControllerMapper.MapFromDto(createSlotDto);
        DomainResult<CreateSlotResult> result = await this.slotFacade.CreateSlot(createRequest, this.HttpContext.RequestAborted);

        return result.MapOk(SlotControllerMapper.ToDto).ToActionResult();
    }

    [HttpGet(Name = nameof(GetAllSlots))]
    [ProducesResponseType(typeof(List<SlotDto>), 200)]
    public async Task<IActionResult> GetAllSlots()
    {
        this.logger.LogTrace("Entering to GetAllSlots");

        DomainResult<IReadOnlyList<Core.Services.Contracts.Entities.SlotEntity>> result = await this.slotFacade.GetAllSlots(this.HttpContext.RequestAborted);

        return result.MapOk(SlotControllerMapper.ToDto).ToActionResult();
    }
}
