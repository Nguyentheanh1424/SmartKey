using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Common.Helpers;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class ForgotPasswordVerifyCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordVerifyCommandValidator
        : AbstractValidator<ForgotPasswordVerifyCommand>
    {
        public ForgotPasswordVerifyCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .Must(BeGmail).WithMessage("Chỉ hỗ trợ email @gmail.com.");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("OTP không được để trống.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ hoa.")
                .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường.")
                .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");
        }

        private bool BeGmail(string email)
        {
            return email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ForgotPasswordVerifyCommandHandler
        : IRequestHandler<ForgotPasswordVerifyCommand, Result>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly IOtpService _otpService;
        private readonly IUnitOfWork _uow;

        public ForgotPasswordVerifyCommandHandler(
            IUnitOfWork uow,
            IOtpService otpService)
        {
            _userRepository = uow.GetRepository<User, Guid>();
            _authRepository = uow.GetRepository<UserAuth, Guid>();
            _otpService = otpService;
            _uow = uow;
        }

        public async Task<Result> Handle(
            ForgotPasswordVerifyCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();
            var otpKey = $"forgot-password:{email}";

            var isValidOtp = await _otpService.VerifyAsync(
                otpKey,
                request.Otp);

            if (!isValidOtp)
                throw new UnauthorizedException("OTP không hợp lệ hoặc đã hết hạn.");

            await _otpService.ClearAttemptAsync(otpKey);

            var user = await _userRepository.FirstOrDefaultAsync(u =>
                u.Email == email)
                ?? throw new NotFoundException("Không tìm thấy người dùng.");

            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == user.Id &&
                a.Provider == AccountProvider.Local.ToString())
                ?? throw new NotFoundException("Không tìm thấy thông tin xác thực.");

            var (hash, salt) = PasswordHasher.Hash(request.NewPassword);

            auth.SetPassword(hash, salt);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.");
        }
    }
}
