using Newtonsoft.Json;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorBatteryMessageHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;

        public DoorBatteryMessageHandler(IUnitOfWork uow)
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

            await _uow.SaveChangesAsync(ct);
        }
    }
}
