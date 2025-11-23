using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AnalysisService.Messaging
{
    public class RabbitMQConsumer : BackgroundService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _hostname;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly Func<string, Task> _messageHandler;

        public RabbitMQConsumer(string hostname, string exchangeName, string queueName, Func<string, Task> messageHandler)
        {
            Console.WriteLine($"[RabbitMQConsumer] Initializing consumer: Host={hostname}, Exchange={exchangeName}, Queue={queueName}");
            _hostname = hostname;
            _exchangeName = exchangeName;
            _queueName = queueName;
            _messageHandler = messageHandler;
        }

        private async Task<bool> InitializeConnectionAsync(CancellationToken cancellationToken)
        {
            int retryCount = 0;
            int maxRetries = 10;
            
            while (retryCount < maxRetries && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"[RabbitMQConsumer] Attempt {retryCount + 1}/{maxRetries} to connect to RabbitMQ at {_hostname}");
                    
                    var factory = new ConnectionFactory()
                    {
                        HostName = _hostname,
                        UserName = "guest",
                        Password = "guest"
                    };

                    _connection = await factory.CreateConnectionAsync(cancellationToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
                    
                    Console.WriteLine($"[RabbitMQConsumer] Connected successfully");

                    // Declare Dead Letter Exchange and Queue
                    var dlxExchangeName = $"{_exchangeName}.dlx";
                    var dlqQueueName = $"{_queueName}.dlq";
                    
                    await _channel.ExchangeDeclareAsync(exchange: dlxExchangeName, type: ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);
                    await _channel.QueueDeclareAsync(queue: dlqQueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
                    await _channel.QueueBindAsync(queue: dlqQueueName, exchange: dlxExchangeName, routingKey: "", cancellationToken: cancellationToken);
                    Console.WriteLine($"[RabbitMQConsumer] Dead Letter Queue '{dlqQueueName}' configured");

                    // Declare main exchange
                    await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Fanout, durable: true, cancellationToken: cancellationToken);
                    Console.WriteLine($"[RabbitMQConsumer] Exchange '{_exchangeName}' declared");
                    
                    // Declare main queue with DLX configuration
                    var queueArgs = new Dictionary<string, object?>
                    {
                        { "x-dead-letter-exchange", dlxExchangeName },
                        { "x-message-ttl", 86400000 } // 24 hours TTL
                    };
                    await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: queueArgs, cancellationToken: cancellationToken);
                    Console.WriteLine($"[RabbitMQConsumer] Queue '{_queueName}' declared with DLX");
                    
                    await _channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: "", cancellationToken: cancellationToken);
                    Console.WriteLine($"[RabbitMQConsumer] Queue '{_queueName}' bound to exchange '{_exchangeName}'");
                    
                    return true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[RabbitMQConsumer] Connection attempt {retryCount} failed: {ex.Message}");
                    
                    if (retryCount < maxRetries)
                    {
                        int delaySeconds = Math.Min(30, (int)Math.Pow(2, retryCount));
                        Console.WriteLine($"[RabbitMQConsumer] Retrying in {delaySeconds} seconds...");
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                    }
                }
            }
            
            Console.WriteLine($"[RabbitMQConsumer] Failed to connect after {maxRetries} attempts");
            return false;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"[RabbitMQConsumer] Starting consumer for queue '{_queueName}'");
            
            // Initialize connection with retry
            bool connected = await InitializeConnectionAsync(stoppingToken);
            if (!connected || _channel == null)
            {
                Console.WriteLine($"[RabbitMQConsumer] Failed to establish connection, exiting");
                return;
            }
            
            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[RabbitMQConsumer] Message received: {message}");
                
                // Get retry count from message headers
                int retryCount = 0;
                if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("x-retry-count"))
                {
                    retryCount = Convert.ToInt32(ea.BasicProperties.Headers["x-retry-count"]);
                }
                
                const int maxRetries = 3;
                
                try
                {
                    await _messageHandler(message);
                    
                    if (_channel != null)
                    {
                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        Console.WriteLine($"[RabbitMQConsumer] Message acknowledged");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQConsumer] Error processing message (Retry {retryCount}/{maxRetries}): {ex.Message}");
                    
                    if (_channel != null)
                    {
                        if (retryCount < maxRetries)
                        {
                            // Increment retry count and requeue
                            var headers = new Dictionary<string, object?>
                            {
                                { "x-retry-count", retryCount + 1 }
                            };
                            
                            // Requeue with updated retry count
                            await _channel.BasicPublishAsync(
                                exchange: _exchangeName,
                                routingKey: "",
                                body: body,
                                mandatory: false
                            );
                            
                            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                            Console.WriteLine($"[RabbitMQConsumer] Message requeued with retry count {retryCount + 1}");
                        }
                        else
                        {
                            // Max retries exceeded, send to Dead Letter Queue
                            await _channel.BasicRejectAsync(deliveryTag: ea.DeliveryTag, requeue: false);
                            Console.WriteLine($"[RabbitMQConsumer] ⚠️ Max retries exceeded. Message sent to Dead Letter Queue.");
                        }
                    }
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
            Console.WriteLine($"[RabbitMQConsumer] Consumer registered and listening...");

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
