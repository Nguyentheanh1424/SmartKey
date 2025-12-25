using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.PasscodeFeatures.Commands;
using SmartKey.Application.Features.PasscodeFeatures.Queries;
using SmartKey.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/doors/{doorId:guid}/passcodes")]
    [Authorize]
    public class PasscodesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PasscodesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh sách passcode",
            Description = "Lấy danh sách passcode hiện tại của cửa (theo DB sync từ device)."
        )]
        public async Task<IActionResult> Get(Guid doorId)
        {
            var result = await _mediator.Send(new GetPasscodesQuery(doorId));
            return Ok(result);
        }

        [HttpPost("add")]
        [SwaggerOperation(
            Summary = "Thêm passcode",
            Description = "Gửi intent thêm passcode xuống thiết bị qua MQTT."
        )]
        public async Task<IActionResult> Add(
            Guid doorId,
            [FromBody] AddPasscodeRequest body)
        {
            var command = new AddPasscodeCommand(
                DoorId: doorId,
                Code: body.Code,
                Type: body.Type,
                ValidFrom: body.ValidFrom,
                ValidTo: body.ValidTo
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("update")]
        [SwaggerOperation(
            Summary = "Cập nhật passcode",
            Description = "Update passcode = remove cũ + add mới."
        )]
        public async Task<IActionResult> Update(
            Guid doorId,
            [FromBody] UpdatePasscodeRequest body)
        {
            var command = new UpdatePasscodeCommand(
                DoorId: doorId,
                Code: body.Code,
                Type: body.Type,
                ValidFrom: body.ValidFrom,
                ValidTo: body.ValidTo
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("delete")]
        [SwaggerOperation(
            Summary = "Xóa passcode",
            Description = "Thu hồi passcode khỏi thiết bị."
        )]
        public async Task<IActionResult> Delete(
            Guid doorId,
            [FromBody] DeletePasscodeRequest body)
        {
            var command = new DeletePasscodeCommand(
                DoorId: doorId,
                Code: body.Code
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("request-sync")]
        [SwaggerOperation(
            Summary = "Yêu cầu đồng bộ passcodes",
            Description = "Yêu cầu thiết bị gửi lại danh sách passcode hiện tại."
        )]
        public async Task<IActionResult> Sync(Guid doorId)
        {
            var result = await _mediator.Send(new SyncPasscodesCommand(doorId));
            return Ok(result);
        }
    }

    public class AddPasscodeRequest
    {
        public string Code { get; init; } = string.Empty;
        public PasscodeType Type { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }

    public class UpdatePasscodeRequest
    {
        public string Code { get; init; } = string.Empty;
        public PasscodeType Type { get; init; }
        public DateTime? ValidFrom { get; init; }
        public DateTime? ValidTo { get; init; }
    }

    public class DeletePasscodeRequest
    {
        public string Code { get; init; } = string.Empty;
    }
}