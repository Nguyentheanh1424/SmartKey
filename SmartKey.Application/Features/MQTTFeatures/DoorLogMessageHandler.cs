using Newtonsoft.Json;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorLogMessageHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;

        public DoorLogMessageHandler(IUnitOfWork uow)
        {
            _uow = uow;
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

            await recordRepo.AddAsync(record);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
