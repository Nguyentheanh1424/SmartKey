using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Common.Helpers;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.UserAuthFeatures.Commands
{
    public class ChangePasswordCommand : IRequest<Result>
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordCommandValidator
        : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty()
                .WithMessage("Mật khẩu hiện tại không được để trống.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
                .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ hoa.")
                .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ thường.")
                .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt.");
        }
    }

    public class ChangePasswordCommandHandler
        : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IRepository<UserAuth, Guid> _authRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _uow;

        public ChangePasswordCommandHandler(
            ICurrentUserService currentUser,
            IUnitOfWork uow)
        {
            _currentUser = currentUser;
            _uow = uow;
            _authRepository = _uow.GetRepository<UserAuth, Guid>();
        }

        public async Task<Result> Handle(
            ChangePasswordCommand request,
            CancellationToken cancellationToken)
        {
            var auth = await _authRepository.FirstOrDefaultAsync(a =>
                a.UserId == _currentUser.UserId &&
                a.Provider == AccountProvider.Local.ToString())
                ?? throw new NotFoundException("Không tìm thấy thông tin xác thực.");

            var (isUsable, message) = auth.IsActive();
            if (!isUsable)
                throw new ForbiddenAccessException(message);

            if (!PasswordHasher.Verify(request.OldPassword, auth.PasswordHash, auth.Salt))
                throw new UnauthorizedException("Mật khẩu cũ không chính xác.");

            var (hash, salt) = PasswordHasher.Hash(request.NewPassword);

            auth.SetPassword(hash, salt);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Đổi mật khẩu thành công. Vui lòng đăng nhập lại.");
        }
    }
}
