using AnalysisService.Data;
using AnalysisService.Events;
using AnalysisService.Models;
using AnalysisService.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AnalysisService.Services
{
    public class ReviewEventHandler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private RabbitMQConsumer? _consumer;

        public ReviewEventHandler(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[ReviewEventHandler] Starting...");
            var rabbitMQHost = _configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
            Console.WriteLine($"[ReviewEventHandler] RabbitMQ Host: {rabbitMQHost}");
            
            _consumer = new RabbitMQConsumer(
                hostname: rabbitMQHost,
                exchangeName: "review.created",
                queueName: "analysis.review.created",
                messageHandler: HandleReviewCreatedAsync
            );

            await _consumer.StartAsync(stoppingToken);
        }

        private async Task HandleReviewCreatedAsync(string message)
        {
            Console.WriteLine($"[ReviewEventHandler] Processing review event: {message}");
            
            var reviewEvent = JsonSerializer.Deserialize<ReviewCreatedEvent>(message);
            
            if (reviewEvent == null)
            {
                Console.WriteLine("[ReviewEventHandler] Failed to deserialize event");
                return;
            }

            Console.WriteLine($"[ReviewEventHandler] Deserialized - ReviewId={reviewEvent.Id}, BusinessId={reviewEvent.BusinessId}");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Perform sentiment analysis (simplified - you can integrate actual ML model)
            var sentimentScore = AnalyzeSentiment(reviewEvent.Comment, reviewEvent.Rating);
            var problems = IdentifyProblems(reviewEvent.Comment);
            var category = DetermineCategory(problems);

            var analysis = new Analysis
            {
                Id = Guid.NewGuid(),
                BusinessId = reviewEvent.BusinessId,
                ReviewId = reviewEvent.Id,
                SentimentScore = sentimentScore,
                ProblemsIdentified = problems,
                Category = category,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Analyses.Add(analysis);
            await dbContext.SaveChangesAsync();

            Console.WriteLine($"[ReviewEventHandler] Analysis created - Id={analysis.Id}, Score={sentimentScore}, Category={category}");

            // TODO: Publish AnalysisCompletedEvent to next service
        }

        private double AnalyzeSentiment(string comment, int rating)
        {
            // Simplified sentiment analysis based on rating
            // In production, use ML.NET or external API
            double baseScore = rating / 5.0;
            
            // Adjust based on keywords
            var lowerComment = comment.ToLower();
            if (lowerComment.Contains("excellent") || lowerComment.Contains("amazing") || lowerComment.Contains("perfect"))
                baseScore += 0.1;
            if (lowerComment.Contains("bad") || lowerComment.Contains("terrible") || lowerComment.Contains("awful"))
                baseScore -= 0.2;

            return Math.Max(0, Math.Min(1, baseScore));
        }

        private List<string> IdentifyProblems(string comment)
        {
            var problems = new List<string>();
            var lowerComment = comment.ToLower();

            if (lowerComment.Contains("slow") || lowerComment.Contains("late"))
                problems.Add("Speed/Timeliness");
            if (lowerComment.Contains("rude") || lowerComment.Contains("unfriendly"))
                problems.Add("Customer Service");
            if (lowerComment.Contains("expensive") || lowerComment.Contains("overpriced"))
                problems.Add("Pricing");
            if (lowerComment.Contains("dirty") || lowerComment.Contains("unclean"))
                problems.Add("Cleanliness");
            if (lowerComment.Contains("quality") || lowerComment.Contains("poor"))
                problems.Add("Quality");

            return problems;
        }

        private string DetermineCategory(List<string> problems)
        {
            if (problems.Count == 0)
                return "Positive";
            if (problems.Count >= 3)
                return "Critical";
            return "NeedsImprovement";
        }

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }
    }
}
