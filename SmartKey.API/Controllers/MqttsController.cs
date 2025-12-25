using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.MQTTFeatures.Commands;
using SmartKey.Application.Features.MQTTFeatures.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("internal/mqtt")]
    [Authorize(Roles = "Admin")]
    public class MqttsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public MqttsController(IMediator mediator) => _mediator = mediator;

        [HttpGet("inbox")]
        [SwaggerOperation(
            Summary = "MQTT inbox",
            Description = "Danh sách message MQTT đã nhận từ device."
        )]
        public async Task<IActionResult> Inbox()
            => Ok(await _mediator.Send(new GetMqttInboxQuery()));

        [HttpGet("inbox/{id:guid}")]
        [SwaggerOperation(
            Summary = "Chi tiết MQTT message",
            Description = "Xem payload, topic và trạng thái xử lý của message."
        )]
        public async Task<IActionResult> InboxDetail(Guid id)
            => Ok(await _mediator.Send(new GetMqttInboxByIdQuery(id)));

        [HttpPost("reprocess/{id:guid}")]
        [SwaggerOperation(
            Summary = "Reprocess MQTT message",
            Description = "Xử lý lại message MQTT (dùng cho debug / recover lỗi)."
        )]
        public async Task<IActionResult> Reprocess(Guid id)
            => Ok(await _mediator.Send(new ReprocessMqttMessageCommand(id)));

        [HttpGet("stats")]
        [SwaggerOperation(
            Summary = "MQTT statistics",
            Description = "Thống kê message MQTT: tổng, đã xử lý, lỗi."
        )]
        public async Task<IActionResult> Stats()
            => Ok(await _mediator.Send(new GetMqttStatsQuery()));
    }
}
