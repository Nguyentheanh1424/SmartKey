using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartKey.Application.Features.UserAuthFeatures.Commands;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartKey.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize]
    public class UserAuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserAuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Đăng ký tài khoản",
            Description = "Lưu thông tin đăng ký vào cache và gửi OTP xác thực email."
        )]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("verify-register-otp")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Xác thực OTP đăng ký",
            Description = "Xác thực OTP và tạo User + UserAuth."
        )]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("resend-register-otp")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Gửi lại OTP đăng ký",
            Description = "Gửi lại OTP đăng ký nếu chưa hết giới hạn."
        )]
        public async Task<IActionResult> ResendRegisterOtp([FromBody] ResendRegisterOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Đăng nhập",
            Description = "Đăng nhập bằng email và mật khẩu."
        )]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password/send-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordSendOtp([FromBody] ForgotPasswordSendOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password/verify")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordVerify([FromBody] ForgotPasswordVerifyCommand command)
        {
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
