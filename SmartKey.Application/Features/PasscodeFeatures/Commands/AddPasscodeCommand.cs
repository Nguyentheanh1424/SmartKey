using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.PasscodeFeatures.Commands
{
    public record AddPasscodeCommand(
        Guid DoorId,
        string Code,
        PasscodeType Type,
        DateTime? ValidFrom,
        DateTime? ValidTo
    ) : IRequest<bool>;

    public class AddPasscodeCommandHandler
        : IRequestHandler<AddPasscodeCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public AddPasscodeCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<bool> Handle(
            AddPasscodeCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(request.DoorId);

            if (door == null)
                throw new Exception("Door not found");

            var payload = new
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
            };

            await _mqtt.PublishPasscodesCommandAsync(
                door.MqttTopicPrefix,
                payload,
                ct
            );

            return true;
        }
    }
}
