using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.DoorCommandFeatures.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/doors/{doorId:guid}")]
    [Authorize]
    public class DoorCommandsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoorCommandsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("lock")]
        [SwaggerOperation(Summary = "Khóa cửa từ app")]
        public async Task<IActionResult> LockDoor(Guid doorId)
        {
            var result = await _mediator.Send(new LockDoorCommand(doorId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("unlock")]
        [SwaggerOperation(Summary = "Mở cửa từ app")]
        public async Task<IActionResult> UnlockDoor(Guid doorId)
        {
            var result = await _mediator.Send(new UnlockDoorCommand(doorId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("sync")]
        [SwaggerOperation(Summary = "Yêu cầu sync trạng thái")]
        public async Task<IActionResult> SyncDoor(Guid doorId)
        {
            var result = await _mediator.Send(new SyncDoorCommand(doorId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
