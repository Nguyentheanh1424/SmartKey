using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.DoorFeatures.Commands
{
    public record DeleteDoorCommand(Guid DoorId) : IRequest<Result>;

    public class DeleteDoorCommandHandler
        : IRequestHandler<DeleteDoorCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public DeleteDoorCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(
            DeleteDoorCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();
            var icCardRepo = _unitOfWork.GetRepository<ICCard, Guid>();
            var passcodeRepo = _unitOfWork.GetRepository<Passcode, Guid>();
            var recordRepo = _unitOfWork.GetRepository<DoorRecord, Guid>();
            var doorCommandRepo = _unitOfWork.GetRepository<DoorCommand, Guid>();
            var mqttInboxRepo = _unitOfWork.GetRepository<MqttInboxMessage, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            if (door.OwnerId != userId)
                throw new ForbiddenAccessException("Bạn không có quyền gỡ cửa này.");

            var mqttMessages = await mqttInboxRepo.FindAsync(x => x.DoorId == door.Id);
            foreach (var msg in mqttMessages)
            {
                await mqttInboxRepo.DeleteAsync(msg);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);  

            var doorCommands = await doorCommandRepo.FindAsync(x => x.DoorId == door.Id);
            foreach (var cmd in doorCommands)
            {
                await doorCommandRepo.DeleteAsync(cmd);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var doorRecords = await recordRepo.FindAsync(x => x.DoorId == door.Id);
            foreach (var record in doorRecords)
            {
                await recordRepo.DeleteAsync(record);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var icCards = await icCardRepo.FindAsync(x => x.DoorId == door.Id);
            foreach (var card in icCards)
            {
                await icCardRepo.DeleteAsync(card);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var passcodes = await passcodeRepo.FindAsync(x => x.DoorId == door.Id);
            foreach (var passcode in passcodes)
            {
                await passcodeRepo.DeleteAsync(passcode);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var shares = await shareRepo.FindAsync(x => x.DoorId == door.Id);
            foreach (var share in shares)
            {
                await shareRepo.DeleteAsync(share);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await doorRepo.DeleteAsync(door);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
