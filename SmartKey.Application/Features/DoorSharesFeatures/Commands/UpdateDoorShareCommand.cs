using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorSharesFeatures.Commands
{
    public record UpdateDoorShareCommand(
        Guid DoorId,
        Guid DoorShareId,
        DoorPermission Permission,
        DateTime? ValidFrom,
        DateTime? ValidTo
    ) : IRequest<Result>;

    public class UpdateDoorShareCommandHandler
        : IRequestHandler<UpdateDoorShareCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateDoorShareCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(
            UpdateDoorShareCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            var share = await shareRepo.GetByIdAsync(request.DoorShareId)
                ?? throw new NotFoundException("DoorShare không tồn tại.");

            if (share.DoorId != door.Id)
                throw new BusinessException("DoorShare không thuộc Door này.");

            DoorActorRole actorRole;

            if (door.OwnerId == currentUserId)
            {
                actorRole = DoorActorRole.Owner;
            }
            else
            {
                var isAdmin = await shareRepo.AnyAsync(x =>
                    x.DoorId == door.Id &&
                    x.UserId == currentUserId &&
                    x.Permission == DoorPermission.Admin &&
                    x.IsActive().isValid);

                if (!isAdmin)
                    throw new ForbiddenAccessException(
                        "Bạn không có quyền cập nhật chia sẻ.");

                actorRole = DoorActorRole.Admin;
            }

            share.UpdatePermission(
                actorUserId: currentUserId,
                actorRole: actorRole,
                newPermission: request.Permission,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
