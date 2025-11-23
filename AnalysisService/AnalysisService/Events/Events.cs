using System;
using System.Text.Json.Serialization;

namespace AnalysisService.Events
{
    public class ReviewCreatedEvent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("businessId")]
        public Guid BusinessId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class AnalysisCompletedEvent
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("businessId")]
        public Guid BusinessId { get; set; }

        [JsonPropertyName("reviewId")]
        public Guid? ReviewId { get; set; }

        [JsonPropertyName("sentimentScore")]
        public double SentimentScore { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("problemsIdentified")]
        public List<string> ProblemsIdentified { get; set; } = new List<string>();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
