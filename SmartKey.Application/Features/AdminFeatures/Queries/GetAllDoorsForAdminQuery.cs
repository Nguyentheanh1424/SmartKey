using AutoMapper;
using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.AdminFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.AdminFeatures.Queries
{
    public record GetAllDoorsForAdminQuery
        : IRequest<Result<List<AdminDoorDto>>>;

    public class GetAllDoorsForAdminQueryHandler
        : IRequestHandler<GetAllDoorsForAdminQuery, Result<List<AdminDoorDto>>>
    {
        private readonly IRepository<Door, Guid> _doorRepository;
        private readonly IMapper _mapper;

        public GetAllDoorsForAdminQueryHandler(
            IUnitOfWork uow,
            IMapper mapper)
        {
            _doorRepository = uow.GetRepository<Door, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<List<AdminDoorDto>>> Handle(
            GetAllDoorsForAdminQuery request,
            CancellationToken cancellationToken)
        {
            var doors = await _doorRepository.GetAllAsync();

            var result = doors
                .Select(door =>
                {
                    var dto = _mapper.Map<AdminDoorDto>(door);
                    dto.State = door.State.ToString();
                    return dto;
                })
                .ToList();

            return Result<List<AdminDoorDto>>.Success(result);
        }
    }
}
