using Newtonsoft.Json;

namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class DoorLogMqttDto
    {
        [JsonProperty("event")]
        public string Event { get; set; } = string.Empty;

        [JsonProperty("method")]
        public string Method { get; set; } = string.Empty;

        [JsonProperty("detail")]
        public string? Detail { get; set; }

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }
    }
}
