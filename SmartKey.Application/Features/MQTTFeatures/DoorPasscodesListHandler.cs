using Newtonsoft.Json;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorPasscodesListHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;

        public DoorPasscodesListHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task HandleAsync(
            Guid doorId,
            string topic,
            string payload,
            CancellationToken ct)
        {
            // Deserialize payload
            PasscodesListMqttDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PasscodesListMqttDto>(payload)!;
            }
            catch (JsonException)
            {
                return;
            }

            if (dto == null)
                return;

            var passcodeRepo = _uow.GetRepository<Passcode, Guid>();

            // Replace-all: load existing passcodes
            var existing = await passcodeRepo.FindAsync(p => p.DoorId == doorId);

            // Delete all existing
            foreach (var p in existing)
            {
                await passcodeRepo.DeleteAsync(p);
            }

            // Insert passcodes from device
            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Code))
                    continue;

                if (!TryMapType(item.Type, out var type))
                    continue;

                var passcode = new Passcode(
                    doorId: doorId,
                    code: item.Code,
                    type: type
                );

                if (type == PasscodeType.Timed)
                {
                    passcode.SetValidity(
                        FromUnix(item.ValidFrom),
                        FromUnix(item.ValidTo)
                    );
                }

                await passcodeRepo.AddAsync(passcode);
            }

            await _uow.SaveChangesAsync(ct);
        }

        private static bool TryMapType(string type, out PasscodeType result)
        {
            switch (type)
            {
                case "one_time":
                    result = PasscodeType.OneTime;
                    return true;

                case "timed":
                    result = PasscodeType.Timed;
                    return true;

                default:
                    result = default;
                    return false;
            }
        }

        private static DateTime? FromUnix(long? seconds)
        {
            if (!seconds.HasValue)
                return null;

            return DateTimeOffset
                .FromUnixTimeSeconds(seconds.Value)
                .UtcDateTime;
        }
    }
}
