using Microsoft.Extensions.Options;
using MQTTnet;

namespace SmartKey.Infrastructure.MQTT
{
    public class MqttOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1883;
        public string ClientId { get; set; } = string.Empty;

        public string? Username { get; set; }
        public string? Password { get; set; }

        public bool UseTls { get; set; }

        public int RetrySeconds { get; set; } = 5;
    }

    public interface IMqttClientOptionsFactory
    {
        MqttClientOptions Create();
    }

    public class MqttClientOptionsFactory : IMqttClientOptionsFactory
    {
        private readonly MqttOptions _options;

        public MqttClientOptionsFactory(IOptions<MqttOptions> options)
        {
            _options = options.Value;
        }

        public MqttClientOptions Create()
        {
            var builder = new MqttClientOptionsBuilder()
                .WithClientId(_options.ClientId)
                .WithTcpServer(_options.Host, _options.Port);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                builder = builder.WithCredentials(
                    _options.Username,
                    _options.Password);
            }

            if (_options.UseTls)
            {
                builder = builder.WithTlsOptions(o => o.UseTls());
            }

            return builder.Build();
        }
    }
}
