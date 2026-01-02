using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Protocol;
using SmartKey.Application.Common.Interfaces.MQTT;
using System.Text.Json;

namespace SmartKey.Infrastructure.MQTT
{
    public class MqttPublisher : IMqttPublisher
    {
        private readonly IMqttClient _client;
        private readonly ILogger<MqttPublisher> _logger;

        private static string PrettyJson(string? json)
        {
            if (json == null) return "";
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public MqttPublisher(IMqttClient client, ILogger<MqttPublisher> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PublishAsync(
            string topic,
            string payload,
            int qos,
            bool retain,
            CancellationToken ct = default)
        {
            if (!_client.IsConnected)
            {
                _logger.LogError(
                    "MQTT publish failed: client not connected. Topic={Topic} QoS={QoS} Retain={Retain} PayloadLength={PayloadLength} Payload={Payload}",
                    topic, qos, retain, payload?.Length ?? 0, PrettyJson(payload));

                throw new InvalidOperationException("MQTT client is not connected.");
            }

            if (qos < 0 || qos > 2)
            {
                _logger.LogError(
                    "MQTT publish failed: invalid QoS. Topic={Topic} QoS={QoS} Retain={Retain} PayloadLength={PayloadLength} Payload={Payload}",
                    topic, qos, retain, payload?.Length ?? 0, PrettyJson(payload));

                throw new ArgumentOutOfRangeException(nameof(qos), qos, "QoS must be 0, 1, or 2.");
            }

            var qosLevel = (MqttQualityOfServiceLevel)qos;

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qosLevel)
                .WithRetainFlag(retain)
                .Build();

            try
            {
                await _client.PublishAsync(message, ct).ConfigureAwait(false);

                _logger.LogInformation(
                    "MQTT published. Topic={Topic} QoS={QoS} Retain={Retain} PayloadLength={PayloadLength} Payload={Payload}",
                    topic, qos, retain, payload?.Length ?? 0, PrettyJson(payload));
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "MQTT publish canceled. Topic={Topic} QoS={QoS} Retain={Retain} PayloadLength={PayloadLength} Payload={Payload}",
                    topic, qos, retain, payload?.Length ?? 0, PrettyJson(payload));

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "MQTT publish exception. Topic={Topic} QoS={QoS} Retain={Retain} PayloadLength={PayloadLength} Payload={Payload}",
                    topic, qos, retain, payload?.Length ?? 0, PrettyJson(payload));

                throw;
            }
        }
    }
}
