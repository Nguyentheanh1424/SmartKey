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
    public record GetDoorByIdQuery(Guid DoorId)
        : IRequest<DoorDetailDto>;

    public class GetDoorByIdQueryHandler
    : IRequestHandler<GetDoorByIdQuery, DoorDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetDoorByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _mapper = mapper;
        }

        public async Task<DoorDetailDto> Handle(
            GetDoorByIdQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException();

            var doorRepo = _unitOfWork.GetRepository<Door, Guid>();
            var shareRepo = _unitOfWork.GetRepository<DoorShare, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door không tồn tại.");

            var dto = _mapper.Map<DoorDetailDto >(door);

            if (door.OwnerId == userId)
            {
                dto.Permission = DoorPermission.Owner;
                dto.ValidFrom = null;
                dto.ValidTo = null;

                return dto;
            }

            var shares = await shareRepo.FindAsync(x =>
                x.DoorId == door.Id &&
                x.UserId == userId);

            var share = shares.FirstOrDefault()
                ?? throw new ForbiddenAccessException("Bạn không có quyền truy cập cửa này.");

            var (isValid, message) = share.IsActive();

            if (!isValid)
                throw new ForbiddenAccessException(message);

            dto.Permission = share.Permission;
            dto.ValidFrom = share.ValidFrom;
            dto.ValidTo = share.ValidTo;

            return dto;
        }
    }
}