using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures.Queries
{
    public record GetMqttInboxQuery()
        : IRequest<List<MqttInboxDto>>;

    public class GetMqttInboxQueryHandler
        : IRequestHandler<GetMqttInboxQuery, List<MqttInboxDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetMqttInboxQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<MqttInboxDto>> Handle(
            GetMqttInboxQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<MqttInboxMessage, Guid>();
            var messages = await repo.GetAllAsync();

            return messages
                .OrderByDescending(x => x.ReceivedAt)
                .Select(x => new MqttInboxDto
                {
                    Id = x.Id,
                    Topic = x.Topic,
                    DoorId = x.DoorId,
                    Fingerprint = x.Fingerprint,
                    ReceivedAt = x.ReceivedAt,
                    IsProcessed = x.IsProcessed
                })
                .ToList();
        }
    }
}
