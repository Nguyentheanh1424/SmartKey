using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Commands
{
    public record UpdateMyAvatarCommand(string AvatarUrl, bool IsRandom = false) : IRequest<Result>;

    public class UpdateMyAvatarCommandValidator
        : AbstractValidator<UpdateMyAvatarCommand>
    {
        public UpdateMyAvatarCommandValidator()
        {
            RuleFor(x => x.AvatarUrl)
                .NotEmpty().WithMessage("Avatar không được để trống.")
                .MaximumLength(10_000)
                .WithMessage("Avatar không hợp lệ.");
        }
    }

    public class UpdateMyAvatarCommandHandler
        : IRequestHandler<UpdateMyAvatarCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IAvatarGenerator _avatarGenerator;

        public UpdateMyAvatarCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IAvatarGenerator avatarGenerator)
        {
            _uow = uow;
            _currentUser = currentUser;
            _avatarGenerator = avatarGenerator;
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            UpdateMyAvatarCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Result.Failure("Không tìm thấy người dùng.");

            if (request.IsRandom)
            {
                var avatarUrl = await _avatarGenerator.GenerateSvgAsync(user.Id.ToString());
                user.UpdateAvatar(avatarUrl);
            }
            else
            {
                user.UpdateAvatar(request.AvatarUrl);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Cập nhật avatar thành công.");
        }
    }
}
