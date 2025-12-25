using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.DoorSharesFeatures.Dtos;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorSharesFeatures.Queries
{
    public record GetDoorSharesQuery(Guid DoorId)
        : IRequest<List<DoorShareDto>>;

    public class GetDoorSharesQueryHandler
        : IRequestHandler<GetDoorSharesQuery, List<DoorShareDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetDoorSharesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<List<DoorShareDto>> Handle(
            GetDoorSharesQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();
            var userRepo = _unitOfWork.GetRepository<User, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            if (door.OwnerId != userId)
            {
                var hasAdminShare = await shareRepo.AnyAsync(x =>
                    x.DoorId == door.Id &&
                    x.UserId == userId &&
                    x.Permission == DoorPermission.Admin &&
                    x.IsActive().isValid);

                if (!hasAdminShare)
                    throw new ForbiddenAccessException("Bạn không có quyền xem danh sách chia sẻ.");
            }

            var shares = await shareRepo.FindAsync(x => x.DoorId == door.Id);

            var result = new List<DoorShareDto>();

            foreach (var share in shares)
            {
                var user = await userRepo.GetByIdAsync(share.UserId);
                if (user == null)
                    continue;

                result.Add(new DoorShareDto
                {
                    Id = share.Id,
                    UserId = user.Id,
                    UserName = user.Name,
                    Email = user.Email,
                    Permission = share.Permission,
                    ValidFrom = share.ValidFrom,
                    ValidTo = share.ValidTo
                });
            }

            return result;
        }
    }
}
