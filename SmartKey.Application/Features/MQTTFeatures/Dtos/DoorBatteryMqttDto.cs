using Newtonsoft.Json;

namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class DoorBatteryMqttDto
    {
        [JsonProperty("battery")]
        public int Battery { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }
    }
}
