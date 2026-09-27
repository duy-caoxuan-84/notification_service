using Microsoft.EntityFrameworkCore;
using NotificationService.Api.Endpoints;
using NotificationService.Core.Contracts;
using NotificationService.Infrastructure.Data;
using Rebus.Config;
using Rebus.Routing.TypeBased;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Configure Rebus
builder.Services.AddRebus(rebus => rebus
    .Logging(l => l.Console())
    .Transport(t => t.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMq"), "notification_api_queue"))
    .Routing(r => r.TypeBased().Map<SendNotificationCommand>("notification_worker_queue"))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map extracted endpoints
app.MapNotificationEndpoints();

app.Run();
