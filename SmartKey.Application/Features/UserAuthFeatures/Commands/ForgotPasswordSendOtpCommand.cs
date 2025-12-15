using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class ForgotPasswordSendOtpCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordSendOtpValidateCommand : AbstractValidator<ForgotPasswordSendOtpCommand>
    {
        public ForgotPasswordSendOtpValidateCommand()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .Must(BeGmail).WithMessage("Chỉ hỗ trợ email @gmail.com.");
        }

        private bool BeGmail(string email)
        {
            return email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ForgotPasswordSendOtpCommandHandler
        : IRequestHandler<ForgotPasswordSendOtpCommand, Result>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public ForgotPasswordSendOtpCommandHandler(
            IUnitOfWork uow,
            IOtpService otpService,
            IEmailService emailService)
        {
            _userRepository = uow.GetRepository<User, Guid>();
            _authRepository = uow.GetRepository<UserAuth, Guid>();
            _otpService = otpService;
            _emailService = emailService;
        }

        public async Task<Result> Handle(
            ForgotPasswordSendOtpCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();
            var otpKey = $"forgot-password:{email}";

            await _otpService.ValidateOtpRequestAsync(otpKey);

            var user = await _userRepository.FirstOrDefaultAsync(u =>
                u.Email == email);

            if (user == null)
                return Result.Success(
                    "Nếu email tồn tại, mã OTP đã được gửi.");

            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == user.Id &&
                a.Provider == AccountProvider.Local.ToString());

            if (auth == null)
                return Result.Success(
                    "Nếu email tồn tại, mã OTP đã được gửi.");

            var otp = await _otpService.GenerateAsync(otpKey);

            await _emailService.SendAsync(
                email,
                "Khôi phục mật khẩu",
                $"Mã OTP của bạn là: {otp}");

            await _otpService.MarkOtpSentAsync(otpKey);

            return Result.Success(
                "Nếu email tồn tại, mã OTP đã được gửi.");
        }

    }
}
