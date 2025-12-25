using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.DoorFeatures.Commands;
using SmartKey.Application.Features.DoorFeatures.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/doors")]
    [Authorize]
    public class DoorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh sách cửa có quyền",
            Description = "Lấy danh sách các cửa mà user hiện tại có quyền truy cập."
        )]
        public async Task<IActionResult> GetMyDoors()
        {
            var result = await _mediator.Send(new GetMyDoorsQuery());
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Thêm cửa mới",
            Description = "Tạo cửa mới và gán owner là user hiện tại."
        )]
        public async Task<IActionResult> CreateDoor(
            [FromBody] CreateDoorCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{doorId:guid}")]
        [SwaggerOperation(
            Summary = "Chi tiết cửa",
            Description = "Lấy thông tin chi tiết của một cửa."
        )]
        public async Task<IActionResult> GetDoorById(Guid doorId)
        {
            var result = await _mediator.Send(new GetDoorByIdQuery(doorId));
            return Ok(result);
        }

        [HttpPut("{doorId:guid}")]
        [SwaggerOperation(
            Summary = "Đổi tên cửa",
            Description = "Owner đổi tên hiển thị của cửa."
        )]
        public async Task<IActionResult> UpdateDoorName(
            Guid doorId,
            [FromBody] UpdateDoorNameRequest body)
        {
            var command = new UpdateDoorNameCommand(
                DoorId: doorId,
                Name: body.Name);

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{doorId:guid}")]
        [SwaggerOperation(
            Summary = "Gỡ cửa",
            Description = "Owner gỡ cửa khỏi hệ thống."
        )]
        public async Task<IActionResult> DeleteDoor(Guid doorId)
        {
            var result = await _mediator.Send(new DeleteDoorCommand(doorId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }

    public class UpdateDoorNameRequest
    {
        public required string Name { get; init; }
    }
}
