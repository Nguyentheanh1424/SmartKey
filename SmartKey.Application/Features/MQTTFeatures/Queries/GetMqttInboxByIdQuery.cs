using MediatR;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures.Queries
{
    public record GetMqttInboxByIdQuery(Guid Id)
        : IRequest<MqttInboxDetailDto?>;

    public class GetMqttInboxByIdQueryHandler
        : IRequestHandler<GetMqttInboxByIdQuery, MqttInboxDetailDto?>
    {
        private readonly IUnitOfWork _uow;

        public GetMqttInboxByIdQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<MqttInboxDetailDto?> Handle(
            GetMqttInboxByIdQuery request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<MqttInboxMessage, Guid>();
            var msg = await repo.GetByIdAsync(request.Id);

            if (msg == null)
                return null;

            return new MqttInboxDetailDto
            {
                Id = msg.Id,
                Topic = msg.Topic,
                Payload = msg.Payload,
                Fingerprint = msg.Fingerprint,
                DoorId = msg.DoorId,
                ReceivedAt = msg.ReceivedAt,
                ProcessedAt = msg.ProcessedAt,
                IsProcessed = msg.IsProcessed
            };
        }
    }
}
