using MediatR;
using Microsoft.Extensions.Configuration;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.UserAuthFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class RefreshTokenCommand : IRequest<Result<LoginResultDto>>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, Result<LoginResultDto>>
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _uow;

        public RefreshTokenCommandHandler(
            IConfiguration configuration,
            ICurrentUserService currentUser,
            ITokenService tokenService,
            IUnitOfWork uow)
        {
            _configuration = configuration;
            _currentUser = currentUser;
            _tokenService = tokenService;
            _uow = uow;
            _authRepository = _uow.GetRepository<UserAuth, Guid>();
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result<LoginResultDto>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken)
        {
            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == _currentUser.UserId &&
                a.Provider == _currentUser.Provider)
                ?? throw new NotFoundException("Không tìm thấy thông tin xác thực.");

            var (isUsable, message) = auth.IsActive();
            if (!isUsable)
                throw new ForbiddenAccessException(message);

            var (isValid, validateMessage) =
                auth.ValidateRefreshToken(request.RefreshToken);
            if (!isValid)
                throw new UnauthorizedException(validateMessage);

            var providerEnum =
                EnumExtensions.ParseEnum<AccountProvider>(auth.Provider);

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == auth.UserId)
                ?? throw new NotFoundException(
                    "Không xác định được người dùng.");

            var (newAccessToken, newRefreshToken) =
                await _tokenService.IssueAsync(auth.UserId, providerEnum, user.Role);

            int refreshDays =
                int.TryParse(_configuration["Jwt:RefreshTokenDays"], out int d)
                    ? d : 7;

            auth.SetRefreshToken(newRefreshToken, refreshDays);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result<LoginResultDto>.Success(new LoginResultDto
            {
                UserId = auth.UserId,
                Provider = auth.Provider,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            }, "Làm mới token thành công");
        }
    }
}
