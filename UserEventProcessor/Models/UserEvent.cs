using System.Text.Json.Serialization;

namespace UserEventProcessor.Models
{
    public class UserEvent
    {
        [JsonPropertyName("userId")]
        public long UserId { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("data")]
        public EventData? Data { get; set; }
    }
}
