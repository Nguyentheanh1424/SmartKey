namespace SmartKey.Application.Common.Interfaces.MQTT
{
    public interface IDoorMqttService
    {
        Task LockAsync(string topicPrefix, CancellationToken ct = default);
        Task UnlockAsync(string topicPrefix, CancellationToken ct = default);

        Task SyncAllAsync(string topicPrefix, CancellationToken ct = default);
        Task RequestPasscodesAsync(string topicPrefix, CancellationToken ct = default);
        Task RequestICCardsAsync(string topicPrefix, CancellationToken ct = default);
        Task RequestBatteryAsync(string topicPrefix, CancellationToken ct = default);

        Task PublishPasscodesCommandAsync(string topicPrefix, object payload, CancellationToken ct = default);

        Task PublishICCardsCommandAsync(string topicPrefix, object payload, CancellationToken ct = default);
    }
}
