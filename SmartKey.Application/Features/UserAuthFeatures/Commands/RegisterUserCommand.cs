using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Cache;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Common.Helpers;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class RegisterUserCommand : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.")
                .Must(BeGmail).WithMessage("Chỉ hỗ trợ email @gmail.com.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.Password)
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

    public class RegisterUserCommandHandler
        : IRequestHandler<RegisterUserCommand, Result>
    {
        private readonly ICacheService _cacheService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;

        public RegisterUserCommandHandler(
            ICacheService cacheService,
            IOtpService otpService,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _cacheService = cacheService;
            _otpService = otpService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();

            bool exists = await _userRepository.AnyAsync(u => u.Email == email);
            if (exists)
                return Result.Failure("Email này đã được sử dụng.");

            await _otpService.ValidateOtpRequestAsync(email);

            var (hash, salt) = PasswordHasher.Hash(request.Password);

            var pendingUser = new PendingUserCacheModel(
                email: email,
                passwordHash: hash,
                salt: salt,
                name: request.Name,
                ttl: TimeSpan.FromMinutes(30)
            );

            await _cacheService.SetAsync(pendingUser);

            var otp = await _otpService.GenerateAsync(email);
            await _otpService.MarkOtpSentAsync(email);

            //await _emailService.SendAsync(
            //    email,
            //    "Xác thực tài khoản SmartKey",
            //    $"<p>Mã OTP của bạn là: <strong>{otp}</strong></p>" +
            //    $"<p>Mã OTP có hiệu lực trong vòng 2 phút. Vui lòng sử dụng nhanh chóng.</p>"
            //);

            Console.WriteLine($"Mã OTP là: {otp}");

            return Result.Success(
                "Đăng ký thành công. Vui lòng kiểm tra email để xác thực OTP.");
        }
    }
}
