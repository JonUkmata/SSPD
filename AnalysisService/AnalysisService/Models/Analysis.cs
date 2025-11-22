using System;
using System.Collections.Generic;

namespace AnalysisService.Models
{
    public class Analysis
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid? ReviewId { get; set; }
        public double SentimentScore { get; set; }
        public List<string> ProblemsIdentified { get; set; } = new List<string>();
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
