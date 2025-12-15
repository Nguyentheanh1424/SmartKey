using AutoMapper;
using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.UserFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.UserFeatures.Queries
{
    public record GetUserByIdQuery(Guid UserId)
        : IRequest<Result<UserDto>>;

    public class GetUserByIdQueryHandler
        : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(
            IUnitOfWork uow,
            IMapper mapper)
        {
            _userRepository = uow.GetRepository<User, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<UserDto>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.FirstOrDefaultAsync(x => x.Id == request.UserId);
            if (user == null)
                return Result<UserDto>.Failure("Không tìm thấy người dùng.");

            var dto = _mapper.Map<UserDto>(user);

            return Result<UserDto>.Success(dto);
        }
    }
}
