using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[Program] Connection string: {connectionString}");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure RabbitMQ Publisher (lazy initialization)
var rabbitMQHost = builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
Console.WriteLine($"[Program] Registering RabbitMQPublisher factory with host: {rabbitMQHost}");
builder.Services.AddSingleton<Func<RabbitMQPublisher>>(sp => 
{
    return () =>
    {
        try
        {
            return new RabbitMQPublisher(rabbitMQHost, "review.created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Program] Failed to create RabbitMQPublisher: {ex.Message}");
            throw;
        }
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("[Program] Applying database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("[Program] Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Program] Failed to apply migrations: {ex.Message}");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disable HTTPS redirection for local testing
// app.UseHttpsRedirection();

app.MapControllers();

app.Run();
