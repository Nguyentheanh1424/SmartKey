using AutoMapper;
using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.AdminFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.AdminFeatures.Queries
{
    public record GetAllUsersForAdminQuery
        : IRequest<Result<List<AdminUserDto>>>;

    public class GetAllUsersForAdminQueryHandler
        : IRequestHandler<GetAllUsersForAdminQuery, Result<List<AdminUserDto>>>
    {
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserAuth, Guid> _userAuthRepository;
        private readonly IMapper _mapper;

        public GetAllUsersForAdminQueryHandler(
            IUnitOfWork uow,
            IMapper mapper)
        {
            _userRepository = uow.GetRepository<User, Guid>();
            _userAuthRepository = uow.GetRepository<UserAuth, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<List<AdminUserDto>>> Handle(
            GetAllUsersForAdminQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _userRepository.GetAllAsync();

            var userAuths = await _userAuthRepository.FindAsync(x => x.Provider == EnumExtensions.GetName(AccountProvider.Local));

            var userAuthDict = userAuths.ToDictionary(x => x.UserId);

            var result = users.Select(user =>
            {
                var dto = _mapper.Map<AdminUserDto>(user);

                if (userAuthDict.TryGetValue(user.Id, out var userAuth))
                {
                    dto.CreatedAt = userAuth.CreatedAt;
                }

                return dto;
            }).ToList();

            return Result<List<AdminUserDto>>.Success(result);
        }
    }
}
