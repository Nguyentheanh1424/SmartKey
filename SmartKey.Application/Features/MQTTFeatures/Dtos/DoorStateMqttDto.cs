using System.Text.Json.Serialization;

namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class DoorStateMqttDto
    {
        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("ts")]
        public long? Timestamp { get; set; }
    }
}
