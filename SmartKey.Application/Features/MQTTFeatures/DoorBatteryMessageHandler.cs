using Newtonsoft.Json;
using SmartKey.Application.Common.Events;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;
using static SmartKey.Application.Features.MQTTFeatures.DoorLogMessageHandler;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorBatteryMessageHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeService _realtimeService;

        public DoorBatteryMessageHandler(IUnitOfWork uow, IRealtimeService realtimeService)
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
            var recordRepo = _uow.GetRepository<DoorRecord, Guid>();

            var door = await doorRepo.GetByIdAsync(doorId);
            if (door == null) return;

            DoorBatteryMqttDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<DoorBatteryMqttDto>(payload)!;
            }
            catch (JsonException)
            {
                return;
            }

            if (dto == null) return;

            if (dto.Battery < 0 || dto.Battery > 100)
                return;

            door.UpdateBattery(dto.Battery);

            var record = new DoorRecord(
                doorId,
                @event: "BatteryUpdated",
                method: "Device",
                rawPayload: payload
            );

            await recordRepo.AddAsync(record);

            await _uow.SaveChangesAsync(ct);

            DoorNotiDetail? notiDetail = new DoorNotiDetail(doorId, door.Name, "BatteryUpdated", "Device");
            notiDetail.Message = "Lưu lượng Pin đã được cập nhật.";

            await _realtimeService.SendNotiToUserAsync(door.OwnerId, MethodType.Notification, notiDetail);
        }
    }
}
