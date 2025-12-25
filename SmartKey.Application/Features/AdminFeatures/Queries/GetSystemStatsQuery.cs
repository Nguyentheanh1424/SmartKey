using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.AdminFeatures.Dtos;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.AdminFeatures.Queries
{
    public record GetSystemStatsQuery
        : IRequest<Result<SystemStatsDto>>;

    public class GetSystemStatsQueryHandler
        : IRequestHandler<GetSystemStatsQuery, Result<SystemStatsDto>>
    {
        private readonly IRepository<User, Guid> _userRepo;
        private readonly IRepository<Door, Guid> _doorRepo;
        private readonly IRepository<DoorRecord, Guid> _recordRepo;
        private readonly IRepository<MqttInboxMessage, Guid> _mqttRepo;

        public GetSystemStatsQueryHandler(IUnitOfWork uow)
        {
            _userRepo = uow.GetRepository<User, Guid>();
            _doorRepo = uow.GetRepository<Door, Guid>();
            _recordRepo = uow.GetRepository<DoorRecord, Guid>();
            _mqttRepo = uow.GetRepository<MqttInboxMessage, Guid>();
        }

        public async Task<Result<SystemStatsDto>> Handle(
            GetSystemStatsQuery request,
            CancellationToken cancellationToken)
        {
            var users = await _userRepo.GetAllAsync();
            var doors = await _doorRepo.GetAllAsync();
            var records = await _recordRepo.GetAllAsync();
            var mqttMessages = await _mqttRepo.GetAllAsync();

            var dto = new SystemStatsDto
            {
                TotalUsers = users.Count,
                TotalDoors = doors.Count,
                TotalDoorRecords = records.Count,

                TotalMqttInbox = mqttMessages.Count,
                PendingMqttInbox = mqttMessages.Count(x => !x.IsProcessed)
            };

            return Result<SystemStatsDto>.Success(dto);
        }
    }
}
