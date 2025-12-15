using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.UserFeatures.Commands;
using SmartKey.Application.Features.UserFeatures.Queries;
using SmartKey.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Lấy profile hiện tại",
            Description = "Lấy thông tin profile của người dùng đang đăng nhập."
        )]
        public async Task<IActionResult> GetMe()
        {
            var result = await _mediator.Send(new GetMyProfileQuery());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("me/name")]
        [SwaggerOperation(
            Summary = "Cập nhật tên",
            Description = "Cập nhật tên hiển thị của người dùng hiện tại." +
                "Người dùng chỉ có thể cập nhật tên sau mỗi 1836 ngày tính từ ngày cập nhật gần nhất."
        )]
        public async Task<IActionResult> UpdateMyName(
            [FromBody] UpdateMyNameCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("me/avatar")]
        [SwaggerOperation(
            Summary = "Cập nhật avatar",
            Description = "Cập nhật avatar cho người dùng hiện tại."
        )]
        public async Task<IActionResult> UpdateMyAvatar(
            [FromBody] UpdateMyAvatarCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("me/phone-number")]
        [SwaggerOperation(
            Summary = "Cập nhật số điện thoại",
            Description = "Cập nhật số điện thoại của người dùng hiện tại."
        )]
        public async Task<IActionResult> UpdateMyPhoneNumber(
            [FromBody] UpdateMyPhoneNumberCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("me/date-of-birth")]
        [SwaggerOperation(
            Summary = "Cập nhật ngày sinh",
            Description = "Cập nhật ngày sinh của người dùng hiện tại."
        )]
        public async Task<IActionResult> UpdateMyDateOfBirth(
            [FromBody] UpdateMyDateOfBirthCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "(Admin) Danh sách tất cả user",
            Description = "Admin xem danh sách toàn bộ user trong hệ thống."
        )]
        public async Task<IActionResult> GetAllUsersForAdmin()
        {
            var result = await _mediator.Send(new GetAllUsersForAdminQuery());
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "(Admin) Xem user khác",
            Description = "Admin xem thông tin profile của user bất kỳ."
        )]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var result = await _mediator.Send(new GetUserByIdQuery(userId));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{userId:guid}/auth-status")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(
            Summary = "(Admin) Cập nhật trạng thái user",
            Description = "Admin thay đổi trạng thái xác thực của user (Active, Locked, Disabled, Banned, Deleted)."
        )]
        public async Task<IActionResult> UpdateUserAuthStatus(
            Guid userId,
            [FromBody] AuthStatus status)
        {
            var result = await _mediator.Send(
                new UpdateUserAuthStatusCommand(userId, status));

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

    }
}
