using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ReviewService.Messaging
{
    public class RabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _exchangeName;

        public RabbitMQPublisher(string hostname, string exchangeName)
        {
            try
            {
                Console.WriteLine($"[RabbitMQPublisher] Initializing: Host={hostname}, Exchange={exchangeName}");
                var factory = new ConnectionFactory()
                {
                    HostName = hostname,
                    UserName = "guest",
                    Password = "guest",
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
                };

                Console.WriteLine($"[RabbitMQPublisher] Creating connection...");
                _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
                Console.WriteLine($"[RabbitMQPublisher] Connection created, creating channel...");
                _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
                _exchangeName = exchangeName;

                Console.WriteLine($"[RabbitMQPublisher] Declaring exchange...");
                // Declare exchange (fanout for broadcasting)
                _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Fanout, durable: true).GetAwaiter().GetResult();
                Console.WriteLine($"[RabbitMQPublisher] Exchange '{exchangeName}' declared successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQPublisher] ERROR: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[RabbitMQPublisher] Inner: {ex.InnerException.Message}");
                throw;
            }
        }

        public async Task PublishAsync<T>(T message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            Console.WriteLine($"[RabbitMQPublisher] Publishing to '{_exchangeName}': {json}");

            await _channel.BasicPublishAsync(exchange: _exchangeName, routingKey: "", body: body);
            Console.WriteLine($"[RabbitMQPublisher] Message published successfully");
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
