using MediatR;
using SmartKey.Application.Common.Exceptions;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.PasscodeFeatures.Commands
{
    public record AddPasscodeCommand(
        Guid DoorId,
        string Code,
        PasscodeType Type,
        DateTime? ValidTo
    ) : IRequest<Result>;

    public class AddPasscodeCommandHandler
        : IRequestHandler<AddPasscodeCommand, Result>
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

        public async Task<Result> Handle(
            AddPasscodeCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var passcodeRepo = _uow.GetRepository<Passcode, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new NotFoundException("Door not found");

            if (string.IsNullOrWhiteSpace(request.Code))
                throw new BusinessException("Passcode is required");

            var existing = await passcodeRepo.FindAsync(p =>
                p.DoorId == request.DoorId &&
                p.Code == request.Code &&
                p.Type == request.Type
            );

            if (request.Type == PasscodeType.Timed)
            {
                if (existing.Any())
                    throw new BusinessException("Passcode already exists");
            }

            if (request.Type == PasscodeType.OneTime)
            {
                if (existing.Any(p => p.IsActive))
                    throw new BusinessException("Active one-time passcode already exists");
            }

            var now = DateTimeOffset.UtcNow;
            var effectiveAtTs = now.ToUnixTimeSeconds();

            long expireAtTs = 0;

            if (request.ValidTo != null)
            {
                var validToUtc = DateTime.SpecifyKind(
                    request.ValidTo.Value,
                    DateTimeKind.Utc
                );

                expireAtTs = new DateTimeOffset(validToUtc)
                    .ToUnixTimeSeconds();

                if (expireAtTs <= effectiveAtTs)
                    throw new BusinessException("ValidTo must be greater than now");
            }

            var ts = effectiveAtTs;

            object payload = request.Type switch
            {
                PasscodeType.OneTime => new
                {
                    action = "add",
                    type = "one_time",
                    code = request.Code,
                    ts,
                    effectiveAt = effectiveAtTs,
                    expireAt = expireAtTs
                },

                PasscodeType.Timed => new
                {
                    action = "add",
                    type = "timed",
                    code = request.Code,
                    ts,
                    effectiveAt = effectiveAtTs,
                    expireAt = expireAtTs
                },

                _ => throw new BusinessException(
                    $"Unsupported passcode type: {request.Type}"
                )
            };

            await _mqtt.PublishPasscodesCommandAsync(
                door.MqttTopicPrefix,
                payload,
                ct
            );

            return Result.Success("Gửi yêu cầu thành công");
        }
    }
}
