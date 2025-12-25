namespace SmartKey.Application.Common.Interfaces.MQTT
{
    public interface IMqttMessageDispatcher
    {
        Task DispatchAsync(
            string topic,
            string payload,
            CancellationToken ct = default);
    }
}
