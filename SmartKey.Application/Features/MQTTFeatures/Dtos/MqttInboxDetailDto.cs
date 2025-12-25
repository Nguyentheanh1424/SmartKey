namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class MqttInboxDetailDto
    {
        public Guid Id { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;

        public string Fingerprint { get; set; } = string.Empty;
        public Guid? DoorId { get; set; }

        public DateTime ReceivedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public bool IsProcessed { get; set; }
    }

}
