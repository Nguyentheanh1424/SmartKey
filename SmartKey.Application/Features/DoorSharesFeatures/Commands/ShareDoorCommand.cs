using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorSharesFeatures.Commands
{
    public record ShareDoorCommand(
        Guid DoorId,
        Guid TargetUserId,
        DoorPermission Permission,
        DateTime? ValidFrom,
        DateTime? ValidTo
    ) : IRequest<Result<Guid>>;

    public class ShareDoorCommandHandler
        : IRequestHandler<ShareDoorCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public ShareDoorCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(
            ShareDoorCommand request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            if (request.TargetUserId == currentUserId)
                throw new BusinessException(
                    "Không thể chia sẻ cửa cho chính mình.");

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
                        "Bạn không có quyền chia sẻ cửa.");

                actorRole = DoorActorRole.Admin;
            }

            if (actorRole == DoorActorRole.Admin &&
                request.Permission == DoorPermission.Admin)
            {
                throw new ForbiddenAccessException(
                    "Admin không thể gán quyền Admin.");
            }

            var existedShare = await shareRepo.AnyAsync(x =>
                x.DoorId == door.Id &&
                x.UserId == request.TargetUserId);

            if (existedShare)
                throw new BusinessException(
                    "Người dùng đã được chia sẻ cửa này.");

            var share = new DoorShare(
                doorId: door.Id,
                userId: request.TargetUserId,
                permission: request.Permission,
                validFrom: request.ValidFrom,
                validTo: request.ValidTo
            );

            await shareRepo.AddAsync(share);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(share.Id);
        }
    }
}
