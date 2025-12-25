using SmartKey.Application.Common.Interfaces.MQTT;
using System.Text.Json;

namespace SmartKey.Infrastructure.MQTT
{
    public class DoorMqttService : IDoorMqttService
    {
        private const int QoS = 1;

        private readonly IMqttPublisher _publisher;

        public DoorMqttService(IMqttPublisher publisher)
        {
            _publisher = publisher;
        }

        public Task LockAsync(string topicPrefix, CancellationToken ct = default)
            => PublishControlAsync(topicPrefix, "lock", ct);

        public Task UnlockAsync(string topicPrefix, CancellationToken ct = default)
            => PublishControlAsync(topicPrefix, "unlock", ct);

        private Task PublishControlAsync(
            string topicPrefix,
            string action,
            CancellationToken ct)
        {
            var payload = JsonSerializer.Serialize(new
            {
                action
            });

            return _publisher.PublishAsync(
                topic: $"{topicPrefix}/control",
                payload: payload,
                qos: QoS,
                retain: true,
                ct: ct);
        }

        public async Task SyncAllAsync(string topicPrefix, CancellationToken ct = default)
        {
            await RequestPasscodesAsync(topicPrefix, ct);
            await RequestICCardsAsync(topicPrefix, ct);
        }

        public Task RequestPasscodesAsync(string topicPrefix, CancellationToken ct = default)
            => _publisher.PublishAsync(
                topic: $"{topicPrefix}/passcodes/request",
                payload: string.Empty,
                qos: QoS,
                retain: false,
                ct: ct);

        public Task RequestICCardsAsync(string topicPrefix, CancellationToken ct = default)
            => _publisher.PublishAsync(
                topic: $"{topicPrefix}/iccards/request",
                payload: string.Empty,
                qos: QoS,
                retain: false,
                ct: ct);

        public Task PublishPasscodesCommandAsync(
            string topicPrefix,
            object payload,
            CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload);

            return _publisher.PublishAsync(
                topic: $"{topicPrefix}/passcodes",
                payload: json,
                qos: QoS,
                retain: false,
                ct: ct);
        }

        public Task PublishICCardsCommandAsync(
            string topicPrefix,
            object payload,
            CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(payload);

            return _publisher.PublishAsync(
                topic: $"{topicPrefix}/iccards",
                payload: json,
                qos: QoS,
                retain: false,
                ct: ct);
        }
    }
}