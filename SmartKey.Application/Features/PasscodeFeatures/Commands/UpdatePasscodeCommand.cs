using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.PasscodeFeatures.Commands
{
    public record UpdatePasscodeCommand(
        Guid DoorId,
        string Code,
        PasscodeType Type,
        DateTime? ValidFrom,
        DateTime? ValidTo
    ) : IRequest<bool>;

    public class UpdatePasscodeCommandHandler
        : IRequestHandler<UpdatePasscodeCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public UpdatePasscodeCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            UpdatePasscodeCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            // Remove old
            await _mqtt.PublishPasscodesCommandAsync(
                door.MqttTopicPrefix,
                new
                {
                    action = "remove",
                    code = request.Code
                },
                ct
            );

            // Add new
            await _mqtt.PublishPasscodesCommandAsync(
                door.MqttTopicPrefix,
                new
                {
                    action = "add",
                    code = request.Code,
                    type = request.Type switch
                    {
                        PasscodeType.OneTime => "one_time",
                        PasscodeType.Timed => "timed",
                        _ => throw new InvalidOperationException()
                    },
                    validFrom = request.ValidFrom,
                    validTo = request.ValidTo
                },
                ct
            );

            return true;
        }
    }
}
