using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.ICCardFeatures.Commands
{
    public record SyncICCardsCommand(
        Guid DoorId
    ) : IRequest<bool>;

    public class SyncICCardsCommandHandler
        : IRequestHandler<SyncICCardsCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public SyncICCardsCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            SyncICCardsCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            await _mqtt.RequestICCardsAsync(
                door.MqttTopicPrefix,
                ct
            );

            return true;
        }
    }
}
