using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.DoorRecordFeatures.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/doors/{doorId:guid}/records")]
    [Authorize]
    public class DoorRecordsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public DoorRecordsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh sách lịch sử cửa",
            Description = "Lấy danh sách record (unlock, lock, wrong passcode, wrong card, ...) của cửa."
        )]
        public async Task<IActionResult> Get(Guid doorId)
        {
            var result = await _mediator.Send(new GetDoorRecordsQuery(doorId));
            return Ok(result);
        }

        [HttpGet("{recordId:guid}")]
        [SwaggerOperation(
            Summary = "Chi tiết record",
            Description = "Xem chi tiết một record cụ thể của cửa."
        )]
        public async Task<IActionResult> GetById(Guid doorId, Guid recordId)
        {
            var result = await _mediator.Send(
                new GetDoorRecordByIdQuery(doorId, recordId));

            return Ok(result);
        }
    }
}
