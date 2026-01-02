using Newtonsoft.Json;
using SmartKey.Application.Common.Events;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;
using System.Net.WebSockets;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorStateMessageHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeService _realtimeService;

        public DoorStateMessageHandler(IUnitOfWork uow, IRealtimeService realtimeService)
        {
            _uow = uow;
            _realtimeService = realtimeService;
        }

        public async Task HandleAsync(
            Guid doorId,
            string topic,
            string payload,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var commandRepo = _uow.GetRepository<DoorCommand, Guid>();
            var doorShare = _uow.GetRepository<DoorShare, Guid>();

            var userIds = (await doorShare.FindAsync(x => x.DoorId == doorId)).Select(x => x.UserId);

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
            if (newState == (null, "")) return;

            door.UpdateState(newState.state);

            var pendingCommand = await commandRepo.FirstOrDefaultAsync(
                c => c.DoorId == doorId &&
                     c.Status == CommandStatus.Pending &&
                     c.CommandType == dto.State);

            if (pendingCommand != null)
            {
                if (IsCommandSatisfied(pendingCommand, newState.state))
                {
                    pendingCommand.MarkSuccess();
                }    
                else
                    pendingCommand.MarkFailed();
            }

            await _uow.SaveChangesAsync(ct);

            var data = new
            {
                type = newState.method,
                message = "Door state had changed!"
            };

            await _realtimeService.SendNotiToUserAsync(door.OwnerId, MethodType.Notification, data);

        }

        private (DoorState state, string method) MapDoorState(string state)
        {
            return state switch
            {
                
                "locked" => (DoorState.Locked, DoorEvents.Locked),
                "unlocked" => (DoorState.Unlocked, DoorEvents.Unlocked),
                _ => (DoorState.Unknown, DoorEvents.Unknown),
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
