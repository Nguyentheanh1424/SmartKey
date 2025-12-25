using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.ICCardFeatures.Commands;
using SmartKey.Application.Features.ICCardFeatures.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/doors/{doorId:guid}/iccards")]
    [Authorize]
    public class ICCardsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ICCardsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh sách thẻ IC",
            Description = "Lấy danh sách thẻ IC hiện tại của cửa (theo DB sync từ device)."
        )]
        public async Task<IActionResult> Get(Guid doorId)
        {
            var result = await _mediator.Send(new GetICCardsQuery(doorId));
            return Ok(result);
        }

        [HttpPost("add")]
        [SwaggerOperation(
            Summary = "Thêm thẻ IC",
            Description = "Gửi intent thêm thẻ IC xuống thiết bị qua MQTT."
        )]
        public async Task<IActionResult> Add(
            Guid doorId,
            [FromBody] AddICCardRequest body)
        {
            var command = new AddICCardCommand(
                DoorId: doorId,
                CardUid: body.CardUid,
                Name: body.Name
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("delete")]
        [SwaggerOperation(
            Summary = "Xóa thẻ IC",
            Description = "Thu hồi thẻ IC khỏi thiết bị."
        )]
        public async Task<IActionResult> Delete(
            Guid doorId,
            [FromBody] DeleteICCardRequest body)
        {
            var command = new DeleteICCardCommand(
                DoorId: doorId,
                CardUid: body.CardUid
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("start-swipe-add")]
        [SwaggerOperation(
            Summary = "Bắt đầu chế độ quẹt thẻ",
            Description = "Yêu cầu thiết bị vào chế độ quẹt thẻ để thêm ICCard mới."
        )]
        public async Task<IActionResult> StartSwipe(Guid doorId)
        {
            var result = await _mediator.Send(
                new StartSwipeAddICCardCommand(doorId));

            return Ok(result);
        }

        [HttpPost("request-sync")]
        [SwaggerOperation(
            Summary = "Yêu cầu đồng bộ thẻ IC",
            Description = "Yêu cầu thiết bị gửi lại danh sách ICCard hiện tại."
        )]
        public async Task<IActionResult> Sync(Guid doorId)
        {
            var result = await _mediator.Send(
                new SyncICCardsCommand(doorId));

            return Ok(result);
        }
    }

    public class AddICCardRequest
    {
        public string CardUid { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }

    public class DeleteICCardRequest
    {
        public string CardUid { get; init; } = string.Empty;
    }
}
