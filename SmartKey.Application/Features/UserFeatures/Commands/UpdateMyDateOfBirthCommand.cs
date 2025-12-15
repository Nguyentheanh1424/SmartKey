using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Commands
{
    public record UpdateMyDateOfBirthCommand(DateTime DateOfBirth) : IRequest<Result>;

    public class UpdateMyDateOfBirthCommandValidator
        : AbstractValidator<UpdateMyDateOfBirthCommand>
    {
        public UpdateMyDateOfBirthCommandValidator()
        {
            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow)
                .WithMessage("Ngày sinh không hợp lệ.");
        }
    }

    public class UpdateMyDateOfBirthCommandHandler
        : IRequestHandler<UpdateMyDateOfBirthCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;

        public UpdateMyDateOfBirthCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            UpdateMyDateOfBirthCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Result.Failure("Không tìm thấy người dùng.");

            user.UpdateDateOfBirth(request.DateOfBirth);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Cập nhật ngày sinh thành công.");
        }
    }
}
