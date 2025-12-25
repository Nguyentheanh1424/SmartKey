namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class MqttInboxDto
    {
        public Guid Id { get; set; }
        public string Topic { get; set; } = string.Empty;
        public Guid? DoorId { get; set; }
        public string Fingerprint { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
        public bool IsProcessed { get; set; }
    }

}
