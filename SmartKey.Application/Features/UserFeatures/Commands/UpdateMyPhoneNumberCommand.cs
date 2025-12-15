using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Commands
{
    public record UpdateMyPhoneNumberCommand(string PhoneNumber) : IRequest<Result>;

    public class UpdateMyPhoneNumberCommandValidator
        : AbstractValidator<UpdateMyPhoneNumberCommand>
    {
        public UpdateMyPhoneNumberCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Số điện thoại không được để trống.")
                .Matches(@"^(0|\+84)[0-9]{9}$")
                .WithMessage("Số điện thoại không hợp lệ.");
        }
    }

    public class UpdateMyPhoneNumberCommandHandler
        : IRequestHandler<UpdateMyPhoneNumberCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;

        public UpdateMyPhoneNumberCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
            _userRepository = _uow.GetRepository<User, Guid>();
        }

        public async Task<Result> Handle(
            UpdateMyPhoneNumberCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Result.Failure("Không tìm thấy người dùng.");

            user.UpdatePhoneNumber(request.PhoneNumber);

            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Cập nhật số điện thoại thành công.");
        }
    }
}
