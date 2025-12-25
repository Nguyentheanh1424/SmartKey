using Microsoft.Extensions.Hosting;
using MQTTnet;

namespace SmartKey.Infrastructure.MQTT
{
    public class MqttHostedService : IHostedService
    {
        private readonly IMqttClient _client;
        private readonly IMqttClientOptionsFactory _optionsFactory;

        public MqttHostedService(
            IMqttClient client,
            IMqttClientOptionsFactory optionsFactory)
        {
            _client = client;
            _optionsFactory = optionsFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_client.IsConnected)
            {
                var options = _optionsFactory.Create();
                await _client.ConnectAsync(options, cancellationToken);
            }
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
