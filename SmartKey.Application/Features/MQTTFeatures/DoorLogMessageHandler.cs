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
            var doorRepo = _uow.GetRepository<Door, Guid>();
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

            var door = await doorRepo.GetByIdAsync(doorId);

            if (door != null)
            {
                var doorName = door?.Name ?? "Cửa không xác định";
                DoorNotiDetail? notiDetail = new DoorNotiDetail(doorId ,doorName, dto.Event, dto.Method);

                switch (dto.Event)
                {
                    case "DoorLocked":
                        notiDetail.Message = $"Cửa đã được khóa.";
                        break;

                    case "DoorUnlocked":
                        notiDetail.Message = $"Cửa đã được mở khóa.";
                        break;

                    case "HandlePasscodeRequestFailed":
                        notiDetail.Message = dto.Detail ?? $"Yêu cầu mã số không thành công.";
                        break;

                    case "MasterCodeAdded":
                        notiDetail.Message = $"Thêm Master Code thành công.";
                        break;

                    case "PasscodeAdded": 
                        notiDetail.Message = $"Thêm Passcode thành công.";
                        break;

                    case "PasscodeDeleted":
                        notiDetail.Message = $"Xóa Passcode thành công";
                        break;

                    case "HandleCardFailed":
                        notiDetail.Message = dto.Detail ?? $"Xử lý Card không thành công.";
                        break;

                    case "CardAdded":
                        notiDetail.Message = $"Thêm Card thành công.";
                        break;

                    case "CardDeleted":
                        notiDetail.Message = $"Xóa Card thành công.";
                        break;

                    case "HandleControlFailed":
                        notiDetail.Message = dto.Detail ?? $"Yêu cầu điều khiển không thành công.";
                        break;

                    case "RelockScheduled":
                        notiDetail.Message = $"Chế độ khóa của cửa tự động {doorName} đã được lên lịch";
                        break;

                    default:
                        // event không quan tâm thì bỏ qua
                        break;
                }

                await _realtimeService.SendNotiToUserAsync(door.OwnerId, MethodType.Notification, notiDetail);
            }

            var record = new DoorRecord(
                doorId,
                @event: dto.Event,
                method: dto.Method,
                rawPayload: payload
            );

            await recordRepo.AddAsync(record);
            await _uow.SaveChangesAsync(ct);
            
        }

        public class DoorNotiDetail
        {
            public Guid DoorId { get; set; }
            public string DoorName { get; set; } = string.Empty;
            public string Event { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;

            public DoorNotiDetail(Guid doorId, string doorName, string @event, string method)
            {
                DoorId = doorId;
                DoorName = doorName;
                Event = @event;
                Method = method;
            }
        }
    }
}
