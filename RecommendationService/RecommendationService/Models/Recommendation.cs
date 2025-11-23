using System;

namespace RecommendationService.Models
{
    public class Recommendation
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid? AnalysisId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Priority { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
