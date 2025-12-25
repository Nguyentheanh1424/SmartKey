using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.DoorSharesFeatures.Commands;
using SmartKey.Application.Features.DoorSharesFeatures.Queries;
using SmartKey.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/doors/{doorId:guid}/shares")]
    [Authorize]
    public class DoorSharesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoorSharesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh sách user được share",
            Description = "Lấy danh sách user được share quyền truy cập cửa."
        )]
        public async Task<IActionResult> GetShares(Guid doorId)
        {
            var result = await _mediator.Send(new GetDoorSharesQuery(doorId));
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Share cửa cho user",
            Description = "Owner share quyền truy cập cửa cho user khác."
        )]
        public async Task<IActionResult> ShareDoor(
            Guid doorId,
            [FromBody] ShareDoorRequest body)
        {
            var command = new ShareDoorCommand(
                DoorId: doorId,
                TargetUserId: body.UserId,
                Permission: body.Permission,
                ValidFrom: body.ValidFrom,
                ValidTo: body.ValidTo
            );

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPut("{userId:guid}")]
        [SwaggerOperation(
            Summary = "Đổi permission / thời hạn",
            Description = "Owner thay đổi quyền hoặc thời hạn share."
        )]
        public async Task<IActionResult> UpdateShare(
            Guid doorId,
            Guid shareId,
            [FromBody] UpdateDoorShareRequest body)
        {
            var command = new UpdateDoorShareCommand(
                DoorId: doorId,
                DoorShareId: shareId,
                Permission: body.Permission,
                ValidFrom: body.ValidFrom,
                ValidTo: body.ValidTo
            );

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpDelete("{userId:guid}")]
        [SwaggerOperation(
            Summary = "Thu hồi quyền",
            Description = "Owner thu hồi quyền truy cập cửa của user."
        )]
        public async Task<IActionResult> RevokeShare(
            Guid doorId,
            Guid shareId)
        {
            var command = new RevokeDoorShareCommand(
                DoorId: doorId,
                DoorShareId: shareId
            );

            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
        }
    }

    public class ShareDoorRequest
    {
        public Guid UserId { get; init; }
        public DoorPermission Permission { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }

    public class UpdateDoorShareRequest
    {
        public DoorPermission Permission { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }

}
