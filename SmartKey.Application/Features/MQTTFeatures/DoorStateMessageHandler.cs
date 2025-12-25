using Newtonsoft.Json;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorStateMessageHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;

        public DoorStateMessageHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task HandleAsync(
            Guid doorId,
            string topic,
            string payload,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var commandRepo = _uow.GetRepository<DoorCommand, Guid>();

            var door = await doorRepo.GetByIdAsync(doorId);
            if (door == null) return;

            DoorStateMqttDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DoorStateMqttDto>(payload)!;
            }
            catch (JsonException)
            {
                return;
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.State))
                return;

            var newState = MapDoorState(dto.State);
            if (newState == null) return;

            door.UpdateState(newState.Value);

            var pendingCommand = await commandRepo.FirstOrDefaultAsync(
                c => c.DoorId == doorId &&
                     c.Status == CommandStatus.Pending);

            if (pendingCommand != null)
            {
                if (IsCommandSatisfied(pendingCommand, newState.Value))
                    pendingCommand.MarkSuccess();
                else
                    pendingCommand.MarkFailed();
            }

            await _uow.SaveChangesAsync(ct);
        }

        private DoorState? MapDoorState(string state)
        {
            return state switch
            {
                "locked" => DoorState.Locked,
                "unlocked" => DoorState.Unlocked,
                _ => null
            };
        }

        private bool IsCommandSatisfied(DoorCommand command, DoorState state)
        {
            return command.CommandType switch
            {
                "lock" => state == DoorState.Locked,
                "unlock" => state == DoorState.Unlocked,
                _ => false
            };
        }
    }
}
