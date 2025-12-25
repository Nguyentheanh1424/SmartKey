using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.ICCardFeatures.Commands
{
    public record StartSwipeAddICCardCommand(
        Guid DoorId
    ) : IRequest<bool>;

    public class StartSwipeAddICCardCommandHandler
        : IRequestHandler<StartSwipeAddICCardCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public StartSwipeAddICCardCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            StartSwipeAddICCardCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            await _mqtt.PublishICCardsCommandAsync(
                door.MqttTopicPrefix,
                new
                {
                    action = "start_swipe_add"
                },
                ct
            );

            return true;
        }
    }
}
