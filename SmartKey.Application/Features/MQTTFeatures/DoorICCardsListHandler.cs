using Newtonsoft.Json;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorICCardsListHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;

        public DoorICCardsListHandler(IUnitOfWork uow)
        {
            _uow = uow;
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

            await _uow.SaveChangesAsync(ct);
        }
    }
}
