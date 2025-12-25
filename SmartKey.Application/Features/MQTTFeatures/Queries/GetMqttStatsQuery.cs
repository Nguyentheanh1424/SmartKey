using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures.Queries
{
    public record GetMqttStatsQuery()
        : IRequest<MqttStatsDto>;

    public class GetMqttStatsQueryHandler
        : IRequestHandler<GetMqttStatsQuery, MqttStatsDto>
    {
        private readonly IUnitOfWork _uow;

        public GetMqttStatsQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<MqttStatsDto> Handle(
            GetMqttStatsQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<MqttInboxMessage, Guid>();
            var all = await repo.GetAllAsync();

            return new MqttStatsDto
            {
                Total = all.Count,
                Processed = all.Count(x => x.IsProcessed),
                Pending = all.Count(x => !x.IsProcessed),

                WithDoor = all.Count(x => x.DoorId.HasValue),
                WithoutDoor = all.Count(x => !x.DoorId.HasValue)
            };
        }
    }
}
