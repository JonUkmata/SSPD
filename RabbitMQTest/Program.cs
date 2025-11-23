using RabbitMQ.Client;
using System.Text;

Console.WriteLine("Testing RabbitMQ Connection...");

try
{
    var factory = new ConnectionFactory()
    {
        HostName = "localhost",
        UserName = "guest",
        Password = "guest",
        RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
    };

    Console.WriteLine("Creating connection...");
    var connection = await factory.CreateConnectionAsync();
    Console.WriteLine("✓ Connection created successfully!");

    Console.WriteLine("Creating channel...");
    var channel = await connection.CreateChannelAsync();
    Console.WriteLine("✓ Channel created successfully!");

    Console.WriteLine("Declaring exchange...");
    await channel.ExchangeDeclareAsync("test.exchange", ExchangeType.Fanout, durable: true);
    Console.WriteLine("✓ Exchange declared successfully!");

    Console.WriteLine("Publishing test message...");
    var message = "{\"test\": \"Hello RabbitMQ\"}";
    var body = Encoding.UTF8.GetBytes(message);
    await channel.BasicPublishAsync("test.exchange", "", body);
    Console.WriteLine("✓ Message published successfully!");

    Console.WriteLine("\n✓✓✓ All RabbitMQ operations successful! ✓✓✓");

    channel.Dispose();
    await connection.CloseAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"\n✗✗✗ ERROR: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"Inner: {ex.InnerException.Message}");
    Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
