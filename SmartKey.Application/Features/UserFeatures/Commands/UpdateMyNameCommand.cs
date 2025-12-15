using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Commands
{
    public record UpdateMyNameCommand(string Name) : IRequest<Result>;

    public class UpdateMyNameCommandValidator
        : AbstractValidator<UpdateMyNameCommand>
    {
        public UpdateMyNameCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.");
        }
    }

    public class UpdateMyNameCommandHandler
        : IRequestHandler<UpdateMyNameCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;

        public UpdateMyNameCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            UpdateMyNameCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Result.Failure("Không tìm thấy người dùng.");

            var (isSuccess, remainingDays) = user.UpdateName(request.Name);

            if (!isSuccess)
            {
                return Result.Failure(
                    $"Bạn có thể đổi tên sau {remainingDays} ngày nữa."
                );
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Cập nhật tên thành công.");
        }
    }
}
