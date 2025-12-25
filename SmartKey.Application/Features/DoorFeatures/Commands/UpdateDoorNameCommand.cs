using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.DoorFeatures.Commands
{
    public record UpdateDoorNameCommand(
        Guid DoorId,
        string Name
    ) : IRequest<Result>;

    public class UpdateDoorNameCommandHandler
        : IRequestHandler<UpdateDoorNameCommand, Result>
    {
        private readonly IRepository<Door, Guid> _doorRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateDoorNameCommandHandler(
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _doorRepository = unitOfWork.GetRepository<Door, Guid>();
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(
            UpdateDoorNameCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var door = await _doorRepository
                .GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            if (door.OwnerId != userId)
                throw new ForbiddenAccessException("Bạn không có quyền đổi tên cửa.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BusinessException("Tên cửa không hợp lệ.");

            door.Rename(request.Name.Trim());

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
