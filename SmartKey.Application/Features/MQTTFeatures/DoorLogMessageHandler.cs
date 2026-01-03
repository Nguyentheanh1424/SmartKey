using Newtonsoft.Json;
using SmartKey.Application.Common.Events;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorLogMessageHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeService _realtimeService;

        public DoorLogMessageHandler(IUnitOfWork uow, IRealtimeService realtimeService  )
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
            var recordRepo = _uow.GetRepository<DoorRecord, Guid>();

            DoorLogMqttDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DoorLogMqttDto>(payload)!;
            }
            catch (JsonException)
            {
                return;
            }

            if (dto == null || string.IsNullOrWhiteSpace(dto.Event))
                return;

            var record = new DoorRecord(
                doorId,
                @event: dto.Event,
                method: dto.Method,
                rawPayload: payload
            );

            var doorRepo = _uow.GetRepository<Door, Guid>();
            var door = await doorRepo.GetByIdAsync(doorId);

            if (door != null)
                await _realtimeService.SendNotiToUserAsync(door.OwnerId, MethodType.Notification, dto.Event);

            await recordRepo.AddAsync(record);
            await _uow.SaveChangesAsync(ct);
            
        }
    }
}
