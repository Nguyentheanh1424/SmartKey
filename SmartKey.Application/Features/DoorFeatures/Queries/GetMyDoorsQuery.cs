using AutoMapper;
using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.DoorFeatures.Dtos;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.DoorFeatures.Queries
{
    public record GetMyDoorsQuery()
        : IRequest<List<DoorListItemDto>>;

    public class GetMyDoorsQueryHandler
        : IRequestHandler<GetMyDoorsQuery, List<DoorListItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetMyDoorsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<List<DoorListItemDto>> Handle(
            GetMyDoorsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();

            var now = DateTime.UtcNow;

            var ownedDoors = await doorRepo.FindAsync(x => x.OwnerId == userId);

            var shares = await shareRepo.FindAsync(x =>
                x.UserId == userId &&
                (!x.ValidFrom.HasValue || now >= x.ValidFrom.Value) &&
                (!x.ValidTo.HasValue || now <= x.ValidTo.Value));

            var result = new List<DoorListItemDto>();

            foreach (var door in ownedDoors)
            {
                var dto = _mapper.Map<DoorListItemDto>(door);
                dto.Permission = DoorPermission.Owner;
                dto.ValidFrom = null;
                dto.ValidTo = null;

                result.Add(dto);
            }

            foreach (var share in shares)
            {
                var door = await doorRepo.GetByIdAsync(share.DoorId);
                if (door == null)
                    continue;

                if (result.Any(x => x.Id == door.Id))
                    continue;

                var dto = _mapper.Map<DoorListItemDto>(door);
                dto.Permission = share.Permission;
                dto.ValidFrom = share.ValidFrom;
                dto.ValidTo = share.ValidTo;

                result.Add(dto);
            }

            return result;
        }
    }
}
