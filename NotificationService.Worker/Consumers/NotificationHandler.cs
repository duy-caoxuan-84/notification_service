using Microsoft.Extensions.Logging;
using NotificationService.Core.Contracts;
using NotificationService.Core.Enums;
using NotificationService.Core.Interfaces;
using NotificationService.Infrastructure.Data;
using Rebus.Handlers;

namespace NotificationService.Worker.Consumers;

public class NotificationHandler : IHandleMessages<SendNotificationCommand>
{
    private readonly ILogger<NotificationHandler> _logger;
    private readonly IEmailProvider _emailProvider;
    private readonly IRateLimiter _rateLimiter;
    private readonly NotificationDbContext _dbContext;

    public NotificationHandler(ILogger<NotificationHandler> logger, IEmailProvider emailProvider, IRateLimiter rateLimiter, NotificationDbContext dbContext)
    {
        _logger = logger;
        _emailProvider = emailProvider;
        _rateLimiter = rateLimiter;
        _dbContext = dbContext;
    }

    public async Task Handle(SendNotificationCommand message)
    {
        _logger.LogInformation("Processing Notification {NotificationId} for User {UserId} via {Channel}", 
            message.NotificationId, message.UserId, message.Channel);

        // Rate Limit Check
        var rateLimitKey = $"rate_limit:{message.Channel}:{message.UserId}";
        var isAllowed = await _rateLimiter.IsAllowedAsync(rateLimitKey, limit: 2, window: TimeSpan.FromMinutes(1));

        if (!isAllowed)
        {
            _logger.LogWarning("RATE LIMIT EXCEEDED for {UserId}. Dropping notification {NotificationId}.", message.UserId, message.NotificationId);
            await UpdateStatusAsync(message.NotificationId, NotificationStatus.Failed, "Rate Limit Exceeded");
            return;
        }

        try
        {
            if (message.Channel == NotificationChannel.Email)
            {
                var targetEmail = $"{message.UserId}@example.com";
                await _emailProvider.SendEmailAsync(targetEmail, "System Notification", message.Payload);
                
                _logger.LogInformation("Successfully sent Email to {Email} for {NotificationId}", targetEmail, message.NotificationId);
                await UpdateStatusAsync(message.NotificationId, NotificationStatus.Sent, "250 OK Delivered to SMTP");
            }
            else 
            {
                _logger.LogWarning("Channel {Channel} is not implemented for {NotificationId}", message.Channel, message.NotificationId);
                await UpdateStatusAsync(message.NotificationId, NotificationStatus.Failed, $"Channel {message.Channel} not supported");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId}", message.NotificationId);
            await UpdateStatusAsync(message.NotificationId, NotificationStatus.Failed, ex.Message);
            throw; // Rethrow to let Rebus retry logic handle it
        }
    }

    private async Task UpdateStatusAsync(Guid notificationId, NotificationStatus status, string providerResponse)
    {
        var record = await _dbContext.Notifications.FindAsync(notificationId);
        if (record != null)
        {
            record.Status = status;
            record.ProviderResponse = providerResponse;
            record.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
