using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AnalysisService.Services
{
    public class DeadLetterQueueMonitor : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public DeadLetterQueueMonitor(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Wait for main consumer
            
            Console.WriteLine("[DeadLetterQueueMonitor] Starting...");
            
            var rabbitMQHost = _configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
            var dlqQueueName = "analysis.review.created.dlq";
            
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = rabbitMQHost,
                    UserName = "guest",
                    Password = "guest"
                };

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
                
                Console.WriteLine($"[DeadLetterQueueMonitor] Connected to RabbitMQ");
                
                var consumer = new AsyncEventingBasicConsumer(_channel);
                
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    Console.WriteLine($"[DeadLetterQueueMonitor] 🔴 FAILED MESSAGE DETECTED:");
                    Console.WriteLine($"[DeadLetterQueueMonitor]    Message: {message}");
                    
                    // Log to file or monitoring system
                    await LogFailedMessageAsync(message);
                    
                    // Acknowledge the message from DLQ
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine($"[DeadLetterQueueMonitor]    ✓ Logged and acknowledged");
                };

                await _channel.BasicConsumeAsync(queue: dlqQueueName, autoAck: false, consumer: consumer);
                Console.WriteLine($"[DeadLetterQueueMonitor] Monitoring Dead Letter Queue: {dlqQueueName}");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeadLetterQueueMonitor] Failed to start: {ex.Message}");
            }
        }

        private async Task LogFailedMessageAsync(string message)
        {
            var logDir = Path.Combine(Directory.GetCurrentDirectory(), "FailedMessages");
            Directory.CreateDirectory(logDir);
            
            var fileName = $"failed_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.json";
            var filePath = Path.Combine(logDir, fileName);
            
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Message = message,
                Reason = "Max retries exceeded"
            };
            
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"[DeadLetterQueueMonitor]    Saved to: {filePath}");
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
