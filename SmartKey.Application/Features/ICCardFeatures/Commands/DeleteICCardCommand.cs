using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.ICCardFeatures.Commands
{
    public record DeleteICCardCommand(
        Guid DoorId,
        string CardUid
    ) : IRequest<bool>;

    public class DeleteICCardCommandHandler
        : IRequestHandler<DeleteICCardCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public DeleteICCardCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            DeleteICCardCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            var payload = new
            {
                action = "remove",
                uid = request.CardUid
            };

            await _mqtt.PublishICCardsCommandAsync(
                door.MqttTopicPrefix,
                payload,
                ct
            );

            return true;
        }
    }
}
