using FluentValidation;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartKey.Application.Features.DoorFeatures.Commands
{
    public record UpdateDoorCodeCommand(
        Guid DoorId,
        string DoorCode
    ) : IRequest<Result>;

    public class UpdateDoorCodeCommandValidator
        : AbstractValidator<UpdateDoorCodeCommand>
    {
        public UpdateDoorCodeCommandValidator()
        {
            RuleFor(x => x.DoorCode)
                .NotEmpty()
                    .WithMessage("Door code không được để trống.")
                .Length(6)
                    .WithMessage("Door code phải gồm đúng 6 chữ số.")
                .Matches(@"^\d{6}$")
                    .WithMessage("Door code phải chỉ gồm số.");
        }
    }

    public class UpdateDoorCodeCommandHandler
        : IRequestHandler<UpdateDoorCodeCommand, Result>
    {
        private readonly IRepository<Door, Guid> _doorRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoorCodeCommandHandler(
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _doorRepository = unitOfWork.GetRepository<Door, Guid>();
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateDoorCodeCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var door = await _doorRepository
                .GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            if (door.OwnerId != userId)
                throw new ForbiddenAccessException("Bạn không có quyền đổi mã cửa.");

            door.UpdateDoorCode(request.DoorCode.Trim());

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
