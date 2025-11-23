using System;
using System.Text.Json.Serialization;

namespace NotificationService.Events
{
    public class RecommendationCreatedEvent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("businessId")]
        public Guid BusinessId { get; set; }

        [JsonPropertyName("analysisId")]
        public Guid? AnalysisId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("priority")]
        public int Priority { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("action")]
        public string Action { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
