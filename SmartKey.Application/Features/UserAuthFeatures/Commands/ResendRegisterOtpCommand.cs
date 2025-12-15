using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Cache;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class ResendRegisterOtpCommand() : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResendRegisterOtpCommandValidator
        : AbstractValidator<ResendRegisterOtpCommand>
    {
        public ResendRegisterOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.");
        }
    }

    public class ResendRegisterOtpCommandHandler
        : IRequestHandler<ResendRegisterOtpCommand, Result>
    {
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;

        public ResendRegisterOtpCommandHandler(
            IOtpService otpService,
            IEmailService emailService,
            ICacheService cacheService,
            IUnitOfWork unitOfWork)
        {
            _otpService = otpService;
            _emailService = emailService;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _userRepository = _unitOfWork.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            ResendRegisterOtpCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();

            bool userExists = await _userRepository.AnyAsync(u => u.Email == email);
            if (userExists)
                return Result.Failure(
                    "Gửi lại OTP không thành công. Email này đã được sử dụng.");

            var cacheKey = $"pending-user:{email}";
            var pendingUser =
                await _cacheService.GetAsync<PendingUserCacheModel>(cacheKey);

            if (pendingUser == null)
                return Result.Failure(
                    "Gửi lại OTP không thành công. Thông tin đăng ký không tồn tại hoặc đã hết hạn. Vui lòng đăng ký lại.");

            await _otpService.ValidateOtpRequestAsync(email);

            var otp = await _otpService.GenerateAsync(email);
            await _otpService.MarkOtpSentAsync(email);

            await _emailService.SendAsync(
                email,
                "Xác thực tài khoản SmartKey",
                $"<p>Mã OTP của bạn là: <strong>{otp}</strong></p>" +
                $"<p>Mã OTP có hiệu lực trong vòng 2 phút. Vui lòng sử dụng nhanh chóng.</p>"
            );

            return Result.Success(
                "Gửi lại OTP thành công. Vui lòng kiểm tra email để xác thực OTP.");
        }
    }
}
