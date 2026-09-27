using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Providers;
using NotificationService.Worker.Consumers;
using Rebus.Config;
using StackExchange.Redis;

using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Configure Redis and Rate Limiter
var redisConnStr = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
var options = ConfigurationOptions.Parse(redisConnStr);
options.AbortOnConnectFail = false;
var multiplexer = ConnectionMultiplexer.Connect(options);
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddSingleton<IRateLimiter, RedisRateLimiter>();

// Configure Email Provider
var smtpHost = builder.Configuration["Smtp:Host"] ?? "localhost";
var smtpPort = int.Parse(builder.Configuration["Smtp:Port"] ?? "1025");
builder.Services.AddSingleton<IEmailProvider>(new SmtpEmailProvider(smtpHost, smtpPort));

// Register all Rebus handlers
builder.Services.AutoRegisterHandlersFromAssemblyOf<NotificationHandler>();

builder.Services.AddRebus(rebus => rebus
    .Logging(l => l.Console())
    .Transport(t => t.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMq"), "notification_worker_queue"))
);

var host = builder.Build();
host.Run();
