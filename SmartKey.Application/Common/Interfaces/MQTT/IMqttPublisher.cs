namespace SmartKey.Application.Common.Interfaces.MQTT
{
    public interface IMqttPublisher
    {
        Task PublishAsync(
            string topic,
            string payload,
            int qos,
            bool retain,
            CancellationToken ct = default);
    }

}
