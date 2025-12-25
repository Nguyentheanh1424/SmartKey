using Newtonsoft.Json;

namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class PasscodesListMqttDto
    {
        [JsonProperty("items")]
        public List<PasscodeItemMqttDto> Items { get; set; } = new();

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }
    }

    public class PasscodeItemMqttDto
    {
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty; // "permanent" | "one_time" | "timed"

        [JsonProperty("validFrom")]
        public long? ValidFrom { get; set; }

        [JsonProperty("validTo")]
        public long? ValidTo { get; set; }
    }
}
