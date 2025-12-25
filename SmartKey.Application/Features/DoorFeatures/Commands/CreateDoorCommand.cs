using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.DoorFeatures.Commands
{
    public record CreateDoorCommand(
        string DoorCode,
        string Name,
        string MqttTopicPrefix
    ) : IRequest<Result<Guid>>;

    public class CreateDoorCommandHandler
        : IRequestHandler<CreateDoorCommand, Result<Guid>>
    {
        private readonly IRepository<Door, Guid> _doorRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IUnitOfWork _unitOfWork;

        public CreateDoorCommandHandler(
            ICurrentUserService currentUser,
            IUnitOfWork unitOfWork)
        {
            _currentUser = currentUser;
            _unitOfWork = unitOfWork;
            _doorRepository = unitOfWork.GetRepository<Door, Guid>();
        }

        public async Task<Result<Guid>> Handle(
            CreateDoorCommand request,
            CancellationToken cancellationToken)
        {
            var ownerId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var exists = await _doorRepository
                .FirstOrDefaultAsync(x => x.DoorCode == request.DoorCode);

            if (exists != null)
                throw new BusinessException("DoorCode đã tồn tại.");

            var door = new Door(
                ownerId,
                request.DoorCode,
                request.Name,
                request.MqttTopicPrefix
            );

            await _doorRepository.AddAsync(door);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(door.Id);
        }
    }
}
