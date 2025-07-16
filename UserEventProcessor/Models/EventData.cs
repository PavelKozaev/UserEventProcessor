using System.Text.Json.Serialization;

namespace UserEventProcessor.Models
{
    public class EventData
    {
        [JsonPropertyName("buttonId")]
        public string? ButtonId { get; set; }
    }
}
