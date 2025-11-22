using System;

namespace RecommendationService.Models
{
    public class Recommendation
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid? AnalysisId { get; set; }
        public string Type { get; set; }
        public int Priority { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
