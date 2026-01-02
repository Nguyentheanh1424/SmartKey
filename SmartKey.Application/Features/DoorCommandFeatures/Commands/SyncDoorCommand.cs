using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.DoorCommandFeatures.Commands
{
    public record SyncDoorCommand(
        Guid DoorId
    ) : IRequest<Result>;

    public class SyncDoorCommandHandler
        : IRequestHandler<SyncDoorCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IDoorMqttService _doorMqttService;

        public SyncDoorCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IDoorMqttService doorMqttService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _doorMqttService = doorMqttService;
        }

        public async Task<Result> Handle(
            SyncDoorCommand request,
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
                    "Bạn không có quyền đồng bộ cửa.");

            await _doorMqttService.SyncAllAsync(
                door.MqttTopicPrefix,
                cancellationToken);

            door.MarkSyncRequested();

            await doorCmdRepo.AddAsync(new DoorCommand(request.DoorId, "sync", "Đồng bộ trạng thái"));

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success("Đã gửi yêu cầu đồng bộ cửa.");
        }
    }
}
