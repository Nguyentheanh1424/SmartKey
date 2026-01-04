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
    public class DoorICCardsListHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeService _realtimeService;

        public DoorICCardsListHandler(IUnitOfWork uow, IRealtimeService realtimeService)
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
            ICCardsListMqttDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<ICCardsListMqttDto>(payload)!;
            }
            catch (JsonException)
            {
                return;
            }

            if (dto == null)
                return;

            var icCardRepo = _uow.GetRepository<ICCard, Guid>();
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var recordRepo = _uow.GetRepository<DoorRecord, Guid>();

            var existing = await icCardRepo.FindAsync(c => c.DoorId == doorId);

            foreach (var card in existing)
            {
                await icCardRepo.DeleteAsync(card);
            }

            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Uid))
                    continue;

                var card = new ICCard(
                    doorId: doorId,
                    uid: item.Uid,
                    name: item.Name ?? string.Empty
                );

                await icCardRepo.AddAsync(card);
            }

            var record = new DoorRecord(
                doorId,
                @event: "CardListUpdated",
                method: "Device",
                rawPayload: payload
            );

            await recordRepo.AddAsync(record);

            await _uow.SaveChangesAsync(ct);

            var door = await doorRepo.GetByIdAsync(doorId);

            DoorNotiDetail? notiDetail = new DoorNotiDetail(doorId, door.Name, "CardListUpdated", "Device");
            notiDetail.Message = "Danh sách Card đã được cập nhật.";

            //await _realtimeService.SendNotiToUserAsync(door.OwnerId, MethodType.Notification, notiDetail);
        }
    }
}
