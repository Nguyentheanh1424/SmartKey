using Newtonsoft.Json;
using SmartKey.Application.Common.Events;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Common.Interfaces.Services;
using SmartKey.Application.Features.MQTTFeatures.Dtos;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;
using static SmartKey.Application.Features.MQTTFeatures.DoorLogMessageHandler;

namespace SmartKey.Application.Features.MQTTFeatures
{
    public class DoorPasscodesListHandler : IMqttMessageHandler
    {
        private readonly IUnitOfWork _uow;
        public readonly IRealtimeService _realtimeService;

        public DoorPasscodesListHandler(IUnitOfWork uow, IRealtimeService realtimeService)
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
            PasscodesListMqttDto dto;
            try
            {
                dto = JsonConvert.DeserializeObject<PasscodesListMqttDto>(payload)!;
            }
            catch (JsonException)
            {
                return;
            }

            if (dto?.Items == null)
                return;

            var doorRepo = _uow.GetRepository<Door, Guid>();
            var passcodeRepo = _uow.GetRepository<Passcode, Guid>();
            var recordRepo = _uow.GetRepository<DoorRecord, Guid>();

            var door = await doorRepo.GetByIdAsync(doorId);
            if (door == null)
                return;

            var existing = await passcodeRepo.FindAsync(p => p.DoorId == doorId);

            var existingOneTimes = existing
                .Where(p => p.Type == PasscodeType.OneTime && p.IsActive)
                .ToList();

            var existingTimed = existing
                .Where(p => p.Type == PasscodeType.Timed)
                .ToList();

            var incomingCodes = new HashSet<string>(
                dto.Items
                    .Where(i => i.Type == "one_time")
                    .Select(i => i.Code)
            );

            foreach (var p in existingOneTimes)
            {
                if (!incomingCodes.Contains(p.Code))
                {
                    p.Expire(); // IsActive = false
                }
            }

            foreach (var p in existingTimed)
            {
                await passcodeRepo.DeleteAsync(p);
            }

            foreach (var item in dto.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Code))
                    continue;

                if (!TryMapType(item.Type, out var type))
                    continue;

                if (type == PasscodeType.Master)
                {
                    door.UpdateDoorCode(item.Code);
                    continue;
                }

                if (type == PasscodeType.OneTime &&
                    existingOneTimes.Any(p => p.Code == item.Code))
                {
                    continue;
                }

                var passcode = new Passcode(
                    doorId: doorId,
                    code: item.Code,
                    type: type
                );

                passcode.SetValidity(
                    FromUnix(item.EffectiveAt),
                    FromUnix(item.ExpireAt)
                );

                await passcodeRepo.AddAsync(passcode);
            }

            var record = new DoorRecord(
                doorId,
                @event: "PasscodeListUpdated",
                method: "Device",
                rawPayload: payload
            );

            await recordRepo.AddAsync(record);

            await _uow.SaveChangesAsync(ct);

            DoorNotiDetail? notiDetail = new DoorNotiDetail(doorId, door.Name, "PasscodeListUpdated", "Device");
            notiDetail.Message = "Danh sách Passcode đã được cập nhật.";

            await _realtimeService.SendNotiToUserAsync(door.OwnerId, MethodType.Notification, notiDetail);
        }

        private static bool TryMapType(
            string type,
            out PasscodeType result)
        {
            switch (type)
            {
                case "master":
                    result = PasscodeType.Master;
                    return true;

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
            if (!seconds.HasValue || seconds.Value <= 0)
                return null;

            return DateTimeOffset
                .FromUnixTimeSeconds(seconds.Value)
                .UtcDateTime;
        }
    }
}
