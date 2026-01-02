using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorCommandFeatures.Commands
{
    public record LockDoorCommand(
        Guid DoorId
    ) : IRequest<Result>;

    public class LockDoorCommandHandler
        : IRequestHandler<LockDoorCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDoorMqttService _doorMqttService;

        public LockDoorCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDoorMqttService doorMqttService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _doorMqttService = doorMqttService;
        }

        public async Task<Result> Handle(
            LockDoorCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var doorCmdRepo = _unitOfWork.GetRepository<DoorCommand, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            var hasPermission =
                door.OwnerId == currentUserId ||
                await shareRepo.AnyAsync(x =>
                    x.DoorId == door.Id &&
                    x.UserId == currentUserId &&
                    x.IsActive().isValid);

            if (!hasPermission)
                throw new ForbiddenAccessException(
                    "Bạn không có quyền khóa cửa.");

            if (door.State == DoorState.Locked)
            {
                return Result.Success("Door đã ở trạng thái khóa.");
            }

            await doorCmdRepo.AddAsync(new DoorCommand(request.DoorId, "lock", "Khóa cửa từ xa"));

            await _doorMqttService.LockAsync(
                door.MqttTopicPrefix,
                cancellationToken);

            return Result.Success("Đã gửi lệnh khóa cửa.");
        }
    }
}
