using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.AdminFeatures.Commands
{
    public record UpdateUserAuthStatusCommand(
        Guid UserId,
        AuthStatus Status
    ) : IRequest<Result>;

    public class UpdateUserAuthStatusCommandValidator
        : AbstractValidator<UpdateUserAuthStatusCommand>
    {
        public UpdateUserAuthStatusCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId không hợp lệ.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Trạng thái không hợp lệ.");
        }
    }

    public class UpdateUserAuthStatusCommandHandler
        : IRequestHandler<UpdateUserAuthStatusCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<UserAuth, Guid> _authRepository;

        public UpdateUserAuthStatusCommandHandler(IUnitOfWork uow)
        {
            _uow = uow;
            _authRepository = _uow.GetRepository<UserAuth, Guid>();
        }

        public async Task<Result> Handle(
            UpdateUserAuthStatusCommand request,
            CancellationToken cancellationToken)
        {
            var auths = await _authRepository
                .FindAsync(a => a.UserId == request.UserId);

            if (!auths.Any())
                return Result.Failure("Không tìm thấy thông tin xác thực của user.");

            Action<UserAuth> applyStatus = request.Status switch
            {
                AuthStatus.Active => auth => auth.Activate(),
                AuthStatus.Locked => auth => auth.Lock(),
                AuthStatus.Disabled => auth => auth.Disable(),
                AuthStatus.Banned => auth => auth.Ban(),
                AuthStatus.Deleted => auth => auth.Delete(),
                _ => throw new BusinessException("Trạng thái không được hỗ trợ.")
            };

            foreach (var auth in auths)
            {
                applyStatus(auth);

                if (request.Status != AuthStatus.Active)
                    auth.RevokeRefreshToken();

                await _authRepository.UpdateAsync(auth);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Cập nhật trạng thái tài khoản thành công.");
        }
    }
}
