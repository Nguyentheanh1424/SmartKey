using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.AdminFeatures.Commands;
using SmartKey.Application.Features.AdminFeatures.Queries;
using SmartKey.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminsController(IMediator mediator) => _mediator = mediator;


        [HttpGet("users")]
        [SwaggerOperation(
            Summary = "Danh sách user",
            Description = "Admin xem danh sách toàn bộ user trong hệ thống."
        )]
        public async Task<IActionResult> Users()
        {
            var result = await _mediator.Send(new GetAllUsersForAdminQuery());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("users/{userId:guid}")]
        [SwaggerOperation(
            Summary = "Chi tiết user",
            Description = "Admin xem thông tin profile của user bất kỳ."
        )]
        public async Task<IActionResult> UserById(Guid userId)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(userId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("users/{userId:guid}/auth-status")]
        [SwaggerOperation(
            Summary = "Cập nhật trạng thái user",
            Description = "Admin thay đổi trạng thái xác thực của user."
        )]
        public async Task<IActionResult> UpdateUserAuthStatus(
            Guid userId,
            [FromBody] AuthStatus status)
        {
            var result = await _mediator.Send(
                new UpdateUserAuthStatusCommand(userId, status));

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        [HttpGet("doors")]
        [SwaggerOperation(
            Summary = "Danh sách cửa",
            Description = "Admin xem danh sách toàn bộ cửa trong hệ thống."
        )]
        public async Task<IActionResult> Doors()
            => Ok(await _mediator.Send(new GetAllDoorsForAdminQuery()));

        
        [HttpGet("stats")]
        [SwaggerOperation(
            Summary = "System statistics",
            Description = "Thống kê tổng quan hệ thống."
        )]
        public async Task<IActionResult> Stats()
            => Ok(await _mediator.Send(new GetSystemStatsQuery()));
    }
}
