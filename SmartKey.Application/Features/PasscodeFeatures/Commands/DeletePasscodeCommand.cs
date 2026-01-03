using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Common;
using SmartKey.Domain.Entities;
using SmartKey.Domain.Enums;

namespace SmartKey.Application.Features.PasscodeFeatures.Commands
{
    public record DeletePasscodeCommand(
        Guid DoorId,
        string Code,
        PasscodeType Type
    ) : IRequest<Result>;


    public class DeletePasscodeCommandHandler
        : IRequestHandler<DeletePasscodeCommand, Result>
    {
        private readonly IUnitOfWork _uow;
        private readonly IDoorMqttService _mqtt;

        public DeletePasscodeCommandHandler(
            IUnitOfWork uow,
            IDoorMqttService mqtt)
        {
            _uow = uow;
            _mqtt = mqtt;
        }

        public async Task<Result> Handle(
            DeletePasscodeCommand request,
            CancellationToken ct)
        {
            var doorRepo = _uow.GetRepository<Door, Guid>();
            var passcodeRepo = _uow.GetRepository<Passcode, Guid>();

            var door = await doorRepo.GetByIdAsync(request.DoorId)
                ?? throw new Exception("Door not found");

            if (request.Type == PasscodeType.Master)
                throw new Exception("Cannot delete master passcode");

            var passcode = (await passcodeRepo.FindAsync(
                p => p.DoorId == request.DoorId &&
                     p.Code == request.Code &&
                     p.Type == request.Type))
                .FirstOrDefault();

            if (request.Type == PasscodeType.OneTime)
            {
                if (passcode == null)
                    throw new Exception("Passcode not found");

                if (!passcode.IsActive)
                {
                    await passcodeRepo.DeleteAsync(passcode);
                    await _uow.SaveChangesAsync(ct);
                    return Result.Success("Xóa mật khẩu thành công!");
                }

                await _mqtt.PublishPasscodesCommandAsync(
                    door.MqttTopicPrefix,
                    new
                    {
                        action = "delete",
                        code = request.Code,
                        type = "one_time"
                    },
                    ct
                );

                return Result.Success("Gửi yêu cầu xóa mật khẩu tới thiết bị, thông báo khi xóa thành công!");
            }

            if (request.Type == PasscodeType.Timed)
            {
                await _mqtt.PublishPasscodesCommandAsync(
                    door.MqttTopicPrefix,
                    new
                    {
                        action = "delete",
                        code = request.Code,
                        type = "timed"
                    },
                    ct
                );

                return Result.Success("Gửi yêu cầu xóa mật khẩu tới thiết bị, thông báo khi xóa thành công!"); ;
            }

            return Result.Failure("Xóa mật khẩu thất bại!");
        }
    }
}
