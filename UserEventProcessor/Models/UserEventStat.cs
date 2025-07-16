using System.Text.Json.Serialization;

namespace UserEventProcessor.Models
{
    public class UserEventStat
    {
        [JsonPropertyName("userId")]
        public long UserId { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
