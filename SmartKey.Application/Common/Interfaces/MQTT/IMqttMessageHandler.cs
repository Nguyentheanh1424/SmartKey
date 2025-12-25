namespace SmartKey.Application.Common.Interfaces.MQTT
{
    public interface IMqttMessageHandler
    {
        Task HandleAsync(
            Guid doorId,
            string topic,
            string payload,
            CancellationToken ct);
    }
}
