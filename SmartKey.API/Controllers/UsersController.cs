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
        [Consumes("multipart/form-data")]
        [SwaggerOperation(
            Summary = "Cập nhật avatar",
            Description =
                "Upload avatar cho người dùng hiện tại.\n\n" +
                "- Request: PUT multipart/form-data\n" +
                "  - Query: isRandom (bool). Nếu true: hệ thống tự sinh avatar, không cần gửi file.\n" +
                "  - Form-data: file (image/jpeg|image/png|image/webp) khi isRandom=false.\n\n" +
                "- Response: trả về avatarUrl (public URL từ storage). FE hiển thị bằng cách gán trực tiếp vào src của <img> " +
                "hoặc fetch GET đến avatarUrl (không cần token nếu bucket public)."
            )]
        [SwaggerResponse(StatusCodes.Status200OK, "Thành công. Response có avatarUrl để FE dùng trực tiếp.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dữ liệu không hợp lệ.")]
        public async Task<IActionResult> UpdateMyAvatar(
            [FromQuery] bool isRandom,
            [FromForm] UpdateMyAvatarRequest request)
        {
            var command = new UpdateMyAvatarCommand(
                IsRandom: isRandom,
                File: request.File
            );

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }


        public sealed class UpdateMyAvatarRequest
        {
            [SwaggerSchema(Description = "File ảnh avatar")]
            public IFormFile File { get; set; } = default!;
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
    }
}
