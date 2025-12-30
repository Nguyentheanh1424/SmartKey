using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.PasscodeFeatures.Commands
{
    public record DeletePasscodeCommand(
        Guid DoorId,
        string Code
    ) : IRequest<bool>;

    public class DeletePasscodeCommandHandler
        : IRequestHandler<DeletePasscodeCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public DeletePasscodeCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            DeletePasscodeCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var passcodeRepo = _uow.GetRepository<Passcode, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            var passcodes = await passcodeRepo.FindAsync(
                p => p.DoorId == request.DoorId);

            if (!passcodes.Any(p => p.Code == request.Code))
                throw new Exception("Passcode not found");

            await _mqtt.PublishPasscodesCommandAsync(
                door.MqttTopicPrefix,
                new
                {
                    action = "remove",
                    code = request.Code
                },
                ct
            );

            return true;
        }
    }
}
