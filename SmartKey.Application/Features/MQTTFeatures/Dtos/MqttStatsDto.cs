namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class MqttStatsDto
    {
        public int Total { get; set; }
        public int Processed { get; set; }
        public int Pending { get; set; }

        public int WithDoor { get; set; }
        public int WithoutDoor { get; set; }
    }

}
