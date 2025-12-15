using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Cache;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class VerifyRegisterOtpCommand
        : IRequest<Result>
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class VerifyRegisterOtpCommandValidator
        : AbstractValidator<VerifyRegisterOtpCommand>
    {
        public VerifyRegisterOtpCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không hợp lệ.");

            RuleFor(x => x.Otp)
                .NotEmpty().WithMessage("Mã OTP không được để trống.");
        }
    }

    public class VerifyRegisterOtpCommandHandler
        : IRequestHandler<VerifyRegisterOtpCommand, Result>
    {
        private readonly IOtpService _otpService;
        private readonly ICacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserAuth, Guid> _userAuthRepository;
        private readonly IAvatarGenerator _avatarGenerator;

        public VerifyRegisterOtpCommandHandler(
            IOtpService otpService,
            ICacheService cacheService,
            IAvatarGenerator avatarGenerator,
            IUnitOfWork unitOfWork)
        {
            _otpService = otpService;
            _cacheService = cacheService;
            _avatarGenerator = avatarGenerator;
            _unitOfWork = unitOfWork;

            _userRepository = _unitOfWork.GetRepository<User, Guid>();
            _userAuthRepository = _unitOfWork.GetRepository<UserAuth, Guid>();
        }

        public async Task<Result> Handle(
            VerifyRegisterOtpCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email.Trim().ToLower();

            var isValidOtp = await _otpService.VerifyAsync(email, request.Otp);
            if (!isValidOtp)
                return Result.Failure("Mã OTP không đúng hoặc đã hết hạn.");

            var cacheKey = $"pending-user:{email}";
            var pendingUser =
                await _cacheService.GetAsync<PendingUserCacheModel>(cacheKey);

            if (pendingUser == null)
                return Result.Failure(
                    "Thông tin đăng ký không tồn tại hoặc đã hết hạn. Vui lòng đăng ký lại.");

            var avatarSvg = await _avatarGenerator.GenerateSvgAsync(email);

            var user = new User(
                pendingUser.Name,
                pendingUser.Email,
                avatarSvg
            );

            var userId = await _userRepository.AddAsync(user);

            var userAuth = UserAuth.CreateLocal(
                user,
                pendingUser.PasswordHash,
                pendingUser.Salt
            );

            await _userAuthRepository.AddAsync(userAuth);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _cacheService.RemoveAsync(cacheKey);

            return Result.Success("Xác thực OTP thành công. Tài khoản đã được tạo.");
        }
    }
}
