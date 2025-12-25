using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.ICCardFeatures.Commands
{
    public record AddICCardCommand(
        Guid DoorId,
        string CardUid,
        string Name
    ) : IRequest<bool>;

    public class AddICCardCommandHandler
        : IRequestHandler<AddICCardCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public AddICCardCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            AddICCardCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            var payload = new
            {
                action = "add",
                uid = request.CardUid,
                name = request.Name
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
