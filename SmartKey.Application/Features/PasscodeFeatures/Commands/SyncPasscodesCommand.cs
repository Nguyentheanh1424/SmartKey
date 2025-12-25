using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.PasscodeFeatures.Commands
{
    public record SyncPasscodesCommand(Guid DoorId) : IRequest<bool>;

    public class SyncPasscodesCommandHandler
        : IRequestHandler<SyncPasscodesCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public SyncPasscodesCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            SyncPasscodesCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            await _mqtt.RequestPasscodesAsync(
                door.MqttTopicPrefix,
                ct
            );

            return true;
        }
    }
}
