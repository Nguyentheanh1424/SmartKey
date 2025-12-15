using MediatR;
using Microsoft.Extensions.Configuration;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.UserAuthFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Common.Helpers;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class LoginCommand : IRequest<Result<LoginResultDto>>
    {
        public string Type { get; set; } = AccountProvider.Local.ToString();
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
    }

    public class LoginCommandHandler
        : IRequestHandler<LoginCommand, Result<LoginResultDto>>
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly IOAuthVerifier _oAuthVerifier;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;

        public LoginCommandHandler(
            IConfiguration configuration,
            IOAuthVerifier oAuthVerifier,
            ITokenService tokenService,
            IUnitOfWork uow)
        {
            _configuration = configuration;
            _oAuthVerifier = oAuthVerifier;
            _tokenService = tokenService;
            _uow = uow;
            _userRepository = _uow.GetRepository<User, Guid>();
            _authRepository = _uow.GetRepository<UserAuth, Guid>();
        }

        public Task<Result<LoginResultDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            return request.Type.ToLower() switch
            {
                "local" => HandleLocalLogin(request, cancellationToken),
                "google" => HandleGoogleLogin(request, cancellationToken),
                "facebook" => HandleFacebookLogin(request, cancellationToken),
                _ => throw new NotSupportedException(
                    $"Login type '{request.Type}' is not supported.")
            };
        }

        private async Task<Result<LoginResultDto>> HandleLocalLogin(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var email = request.Email!.Trim().ToLower();

            var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new NotFoundException("Email chưa được đăng ký.");

            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == user.Id &&
                a.Provider == EnumExtensions.GetName(AccountProvider.Local))
                ?? throw new AppException("Tài khoản chưa thiết lập đăng nhập Local.");

            var (isUsable, message) = auth.IsActive();
            if (!isUsable)
                throw new ForbiddenAccessException(message);

            var (isLocked, remaining) = auth.IsLocked();
            if (isLocked)
                throw new ForbiddenAccessException(
                    $"Tài khoản bị khóa. Vui lòng thử lại sau {remaining}.");

            var valid = PasswordHasher.Verify(
                request.Password!,
                auth.PasswordHash,
                auth.Salt);

            if (!valid)
            {
                var failMessage = auth.MarkLoginFailed();
                await _authRepository.UpdateAsync(auth);
                await _uow.SaveChangesAsync(cancellationToken);

                throw new UnauthorizedException(
                    $"Mật khẩu không đúng. {failMessage}");
            }

            auth.MarkLoginSuccess();

            var (accessToken, refreshToken) =
                await _tokenService.IssueAsync(user.Id, AccountProvider.Local, user.Role);

            int refreshDays =
                int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int d)
                    ? d : 7;

            auth.SetRefreshToken(refreshToken, refreshDays);
            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<LoginResultDto>.Success(new LoginResultDto
            {
                UserId = user.Id,
                Provider = AccountProvider.Local.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, "Đăng nhập thành công");
        }

        private async Task<Result<LoginResultDto>> HandleGoogleLogin(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var profile = await _oAuthVerifier.VerifyGoogleAsync(request.Token!);
            return await HandleOAuth(profile, AccountProvider.Google, cancellationToken);
        }

        private async Task<Result<LoginResultDto>> HandleFacebookLogin(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var profile = await _oAuthVerifier.VerifyFacebookAsync(request.Token!);
            return await HandleOAuth(profile, AccountProvider.Facebook, cancellationToken);
        }

        private async Task<Result<LoginResultDto>> HandleOAuth(
            OAuthProfileDto profile,
            AccountProvider provider,
            CancellationToken cancellationToken)
        {
            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.Provider == EnumExtensions.GetName(provider) &&
                a.ProviderUid == profile.Uid)
                ?? throw new NotFoundException(
                    "Chưa tạo tài khoản hoặc chưa liên kết phương thức đăng nhập.");

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == auth.UserId)
                ?? throw new NotFoundException(
                    "Không xác định được người dùng.");

            var (accessToken, refreshToken) =
                await _tokenService.IssueAsync(auth.UserId, provider, user.Role);

            int refreshDays =
                int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int d)
                    ? d : 7;

            auth.MarkLoginSuccess();
            auth.SetRefreshToken(refreshToken, refreshDays);

            await _authRepository.UpdateAsync(auth);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<LoginResultDto>.Success(new LoginResultDto
            {
                UserId = auth.UserId,
                Provider = provider.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken
            }, "Đăng nhập thành công");
        }
    }
}
