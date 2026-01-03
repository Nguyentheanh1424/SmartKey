using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.ICCardFeatures.Commands
{
    public record SyncICCardsCommand(
        Guid DoorId
    ) : IRequest<Result>;

    public class SyncICCardsCommandHandler
        : IRequestHandler<SyncICCardsCommand, Result>
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

        public async Task<Result> Handle(
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

            return Result.Success("Gửi yêu cầu đồng bộ thành công.");
        }
    }
}
