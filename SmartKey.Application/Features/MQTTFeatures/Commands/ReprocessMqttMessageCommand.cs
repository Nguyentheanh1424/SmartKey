using MediatR;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Domain.Entities;

namespace SmartKey.Application.Features.MQTTFeatures.Commands
{
    public record ReprocessMqttMessageCommand(Guid Id)
        : IRequest<bool>;

    public class ReprocessMqttMessageCommandHandler
        : IRequestHandler<ReprocessMqttMessageCommand, bool>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMqttMessageDispatcher _dispatcher;

        public ReprocessMqttMessageCommandHandler(
            IUnitOfWork uow,
            IMqttMessageDispatcher dispatcher)
        {
            _uow = uow;
            _dispatcher = dispatcher;
        }

        public async Task<bool> Handle(
            ReprocessMqttMessageCommand request,
            CancellationToken ct)
        {
            var repo = _uow.GetRepository<MqttInboxMessage, Guid>();
            var msg = await repo.GetByIdAsync(request.Id);

            if (msg == null)
                return false;

            await _dispatcher.DispatchAsync(
                msg.Topic,
                msg.Payload,
                ct
            );

            return true;
        }
    }
}
