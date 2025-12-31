using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.Authentication.Commands
{
    public class UnlinkOAuthCommand : IRequest<Result>
    {
        public string Provider { get; set; } = default!;
    }

    public class UnlinkOAuthCommandValidator : AbstractValidator<UnlinkOAuthCommand>
    {
        public UnlinkOAuthCommandValidator()
        {
            RuleFor(x => x.Provider)
                .Must(p => p.Equals("google", StringComparison.OrdinalIgnoreCase)
                        || p.Equals("facebook", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Chỉ cho phép hủy liên kết Google hoặc Facebook.");
        }
    }

    public class UnlinkOAuthCommandHandler
    : IRequestHandler<UnlinkOAuthCommand, Result>
    {
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public UnlinkOAuthCommandHandler(
            ICurrentUserService currentUser,
            IUnitOfWork uow)
        {
            _currentUser = currentUser;
            _uow = uow;
            _authRepository = _uow.GetRepository<UserAuth, Guid>();
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            UnlinkOAuthCommand request,
            CancellationToken cancellationToken)
        {
            var provider = Enum.Parse<AccountProvider>(request.Provider, true);
            var providerName = EnumExtensions.GetName(provider);

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == _currentUser.UserId)
                ?? throw new NotFoundException("Không tìm thấy người dùng.");

            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == user.Id &&
                a.Provider == providerName);

            if (auth == null)
                throw new NotFoundException("Phương thức đăng nhập chưa được liên kết.");

            if (provider == AccountProvider.Local)
                throw new AppException("Không thể hủy liên kết tài khoản Local.");

            await _authRepository.DeleteAsync(auth);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Hủy liên kết tài khoản thành công.");
        }
    }
}
