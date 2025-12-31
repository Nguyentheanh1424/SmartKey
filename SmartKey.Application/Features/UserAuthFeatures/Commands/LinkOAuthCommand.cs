using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.Authentication.Commands
{
    public class LinkOAuthCommand : IRequest<Result>
    {
        public string Provider { get; set; } = default!;
        public string Token { get; set; } = default!;
    }

    public class LinkOAuthCommandValidator : AbstractValidator<LinkOAuthCommand>
    {
        public LinkOAuthCommandValidator()
        {
            RuleFor(x => x.Provider)
                .Must(p => p.Equals("google", StringComparison.OrdinalIgnoreCase)
                        || p.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Provider chỉ được là Google hoặc Facebook.");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("Token không được để trống.");
        }
    }

    public class LinkOAuthCommandHandler
    : IRequestHandler<LinkOAuthCommand, Result>
    {
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IOAuthVerifier _oAuthVerifier;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public LinkOAuthCommandHandler(
            IOAuthVerifier oAuthVerifier,
            ICurrentUserService currentUser,
            IUnitOfWork uow)
        {
            _oAuthVerifier = oAuthVerifier;
            _currentUser = currentUser;
            _uow = uow;
            _authRepository = _uow.GetRepository<UserAuth, Guid>();
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            LinkOAuthCommand request,
            CancellationToken cancellationToken)
        {
            var provider = Enum.Parse<AccountProvider>(request.Provider, true);
            var providerName = EnumExtensions.GetName(provider);

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == _currentUser.UserId)
                ?? throw new NotFoundException("Không tìm thấy người dùng.");

            var existedAuth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == user.Id &&
                a.Provider == providerName);

            if (existedAuth != null)
                throw new AppException("Bạn đã liên kết phương thức đăng nhập này.");

            OAuthProfileDto profile = provider switch
            {
                AccountProvider.Google =>
                    await _oAuthVerifier.VerifyGoogleAsync(request.Token),

                AccountProvider.Facebook =>
                    await _oAuthVerifier.VerifyFacebookAsync(request.Token),

                _ => throw new NotSupportedException()
            };

            var linkedAuth = await _authRepository.FirstOrDefaultAsync(a =>
                a.Provider == providerName &&
                a.ProviderUid == profile.Uid);

            if (linkedAuth != null)
                throw new AppException("Tài khoản mạng xã hội này đã được liên kết.");

            var auth = UserAuth.CreateOAuth(
                user.Id,
                providerName,
                profile.Uid);

            await _authRepository.AddAsync(auth);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Liên kết tài khoản thành công.");
        }
    }
}
