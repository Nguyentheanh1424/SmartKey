using Microsoft.Extensions.Logging;
using MQTTnet;
using SmartKey.Application.Common.Interfaces.MQTT;

namespace SmartKey.Infrastructure.MQTT
{
    public class MqttPublisher : IMqttPublisher
    {
        private readonly IMqttClient _client;
        private readonly ILogger<MqttPublisher> _logger;

        public MqttPublisher(
            IMqttClient client,
            ILogger<MqttPublisher> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task PublishAsync(
            string topic,
            string payload,
            int qos,
            bool retain,
            CancellationToken ct = default)
        {
            if (!_client.IsConnected)
                throw new InvalidOperationException("MQTT client is not connected.");

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                .WithRetainFlag(retain)
                .Build();

            await _client.PublishAsync(message, ct);
        }
    }

}
