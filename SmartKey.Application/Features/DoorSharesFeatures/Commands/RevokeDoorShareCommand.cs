using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorSharesFeatures.Commands
{
    public record RevokeDoorShareCommand(
        Guid DoorId,
        Guid DoorShareId
    ) : IRequest<Result>;

    public class RevokeDoorShareCommandHandler
        : IRequestHandler<RevokeDoorShareCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public RevokeDoorShareCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(
            RevokeDoorShareCommand request,
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
                throw new BusinessException(
                    "DoorShare không thuộc Door này.");

            if (share.UserId == currentUserId)
                throw new BusinessException(
                    "Không thể thu hồi quyền của chính mình.");

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
                        "Bạn không có quyền thu hồi chia sẻ.");

                actorRole = DoorActorRole.Admin;
            }

            if (actorRole == DoorActorRole.Admin &&
                share.Permission == DoorPermission.Admin)
            {
                throw new ForbiddenAccessException(
                    "Admin không thể thu hồi quyền Admin khác.");
            }

            await shareRepo.DeleteAsync(share);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
