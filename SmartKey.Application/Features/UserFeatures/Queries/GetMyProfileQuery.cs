using AutoMapper;
using MediatR;
using SmartKey.Application.Common.Interfaces.Auth;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.UserFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Queries
{
    public record GetMyProfileQuery : IRequest<Result<UserDto>>;

    public class GetMyProfileQueryHandler
        : IRequestHandler<GetMyProfileQuery, Result<UserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IMapper _mapper;

        public GetMyProfileQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            IMapper mapper)
        {
            _currentUser = currentUser;
            _mapper = mapper;
            _userRepository = uow.GetRepository<User, Guid>();
        }

        public async Task<Result<UserDto>> Handle(
            GetMyProfileQuery request,
            CancellationToken cancellationToken)
        {

            var userId = _currentUser.UserId;

            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return Result<UserDto>.Failure("Không tìm thấy người dùng.");

            var dto = _mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(dto);
        }
    }
}
