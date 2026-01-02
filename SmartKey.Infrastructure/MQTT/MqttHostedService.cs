using Microsoft.Extensions.Hosting;
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

        public MqttHostedService(
            IMqttClient client,
            IMqttClientOptionsFactory optionsFactory,
            IMqttMessageDispatcher dispatcher)
        {
            _client = client;
            _optionsFactory = optionsFactory;
            _dispatcher = dispatcher;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_client.IsConnected)
            {
                var options = _optionsFactory.Create();
                await _client.ConnectAsync(options, cancellationToken);
            }

            _client.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;

                var payloadSeq = e.ApplicationMessage.Payload;
                var payload = payloadSeq.IsEmpty
                    ? string.Empty
                    : Encoding.UTF8.GetString(payloadSeq.ToArray());

                await _dispatcher.DispatchAsync(topic, payload, cancellationToken);
            };

            await _client.SubscribeAsync("+/state");
            await _client.SubscribeAsync("+/log");
            await _client.SubscribeAsync("+/battery");
            await _client.SubscribeAsync("+/passcodes");
            await _client.SubscribeAsync("+/iccards");
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
