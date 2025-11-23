using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
