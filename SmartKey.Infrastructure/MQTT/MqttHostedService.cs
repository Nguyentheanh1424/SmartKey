using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using SmartKey.Application.Common.Interfaces.MQTT;
using System.Buffers;
using System.Text;

namespace SmartKey.Infrastructure.MQTT
{
    public class MqttHostedService : IHostedService
    {
        private readonly IMqttClient _client;
        private readonly IMqttClientOptionsFactory _optionsFactory;
        private readonly IMqttMessageDispatcher _dispatcher;
        private readonly ILogger<MqttHostedService> _logger;

        private MqttClientOptions? _options;
        private const int RetrySeconds = 5;

        public MqttHostedService(
            IMqttClient client,
            IMqttClientOptionsFactory optionsFactory,
            IMqttMessageDispatcher dispatcher,
            ILogger<MqttHostedService> logger)
        {
            _client = client;
            _optionsFactory = optionsFactory;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _options = _optionsFactory.Create();

            _client.ApplicationMessageReceivedAsync += OnMessageReceived;
            _client.DisconnectedAsync += OnDisconnected;

            await ConnectWithRetryAsync(cancellationToken);

            await _client.SubscribeAsync("+/state");
            await _client.SubscribeAsync("+/log");
            await _client.SubscribeAsync("+/battery");
            await _client.SubscribeAsync("+/passcodes");
            await _client.SubscribeAsync("+/iccards");
        }

        private async Task ConnectWithRetryAsync(CancellationToken cancellationToken)
        {
            while (!_client.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Connecting to MQTT broker...");
                    await _client.ConnectAsync(_options!, cancellationToken);
                    _logger.LogInformation("MQTT connected");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "MQTT connect failed. Retry in {Seconds}s", RetrySeconds);
                    await Task.Delay(TimeSpan.FromSeconds(RetrySeconds), cancellationToken);
                }
            }
        }

        private async Task OnDisconnected(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning(
                "MQTT disconnected. Reason: {Reason}", e.Reason);

            if (e.ClientWasConnected == false)
                return;

            await Task.Delay(TimeSpan.FromSeconds(RetrySeconds));

            try
            {
                if (_options != null)
                {
                    _logger.LogInformation("Reconnecting MQTT...");
                    await _client.ConnectAsync(_options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT reconnect failed");
            }
        }

        private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;

            var payloadSeq = e.ApplicationMessage.Payload;
            var payload = payloadSeq.IsEmpty
                ? string.Empty
                : Encoding.UTF8.GetString(payloadSeq.ToArray());

            await _dispatcher.DispatchAsync(topic, payload, CancellationToken.None);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
