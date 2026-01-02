using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartKey.Application.Common.Interfaces.MQTT;
using SmartKey.Application.Common.Interfaces.Repositories;
using SmartKey.Application.Features.MQTTFeatures;
using SmartKey.Domain.Entities;
using System.Text.Json;

namespace SmartKey.Infrastructure.MQTT
{
    public class MqttMessageDispatcher : IMqttMessageDispatcher
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttMessageDispatcher> _logger;

        private static string PrettyJson(string? json)
        {
            if (json == null) return "";
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public MqttMessageDispatcher(IServiceScopeFactory scopeFactory, ILogger<MqttMessageDispatcher> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private IMqttMessageHandler? ResolveHandler(
            IServiceScope scope,
            string suffix)
        {
            return suffix switch
            {
                "state" => scope.ServiceProvider
                    .GetService<DoorStateMessageHandler>(),

                "battery" => scope.ServiceProvider
                    .GetService<DoorBatteryMessageHandler>(),

                "log" => scope.ServiceProvider
                    .GetService<DoorLogMessageHandler>(),

                "passcodes" => scope.ServiceProvider
                    .GetService<DoorPasscodesListHandler>(),

                "iccards" => scope.ServiceProvider
                    .GetService<DoorICCardsListHandler>(),

                _ => null
            };
        }

        public async Task DispatchAsync(
            string topic,
            string payload,
            CancellationToken ct)
        {
            _logger.LogInformation(
                "MQTT message received.Topic: {Topic} Payload: {Payload}",
                topic,
                PrettyJson(payload));


            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var inboxRepo = uow.GetRepository<MqttInboxMessage, Guid>();

            var fingerprint = MqttFingerprint.Create(topic, payload);

            var exists = await inboxRepo.AnyAsync(
                m => m.Fingerprint == fingerprint);

            if (exists)
                return;

            var parts = topic.Split('/');
            if (parts.Length != 2)
                return;

            var mqttPrefix = parts[0];
            var suffix = parts[1];

            var doorRepo = uow.GetRepository<Door, Guid>();
            var door = await doorRepo.FirstOrDefaultAsync(
                d => d.MqttTopicPrefix == mqttPrefix);

            var inbox = new MqttInboxMessage(
                topic,
                payload,
                fingerprint,
                door?.Id);

            await inboxRepo.AddAsync(inbox);

            if (door != null)
            {
                var handler = ResolveHandler(scope, suffix);
                if (handler != null)
                {
                    await handler.HandleAsync(
                        door.Id,
                        topic,
                        payload,
                        ct);
                }
            }

            inbox.MarkProcessed();
            await uow.SaveChangesAsync(ct);
        }
    }
}
