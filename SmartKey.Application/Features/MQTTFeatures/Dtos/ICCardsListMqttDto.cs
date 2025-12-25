using Newtonsoft.Json;

namespace SmartKey.Application.Features.MQTTFeatures.Dtos
{
    public class ICCardsListMqttDto
    {
        [JsonProperty("items")]
        public List<ICCardItemMqttDto> Items { get; set; } = new();

        [JsonProperty("ts")]
        public long? Timestamp { get; set; }
    }

    public class ICCardItemMqttDto
    {
        [JsonProperty("uid")]
        public string Uid { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
    }
}
