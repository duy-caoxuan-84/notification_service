using NotificationService.Core.Enums;

namespace NotificationService.Api.DTOs;

public class CreateNotificationRequest
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string Payload { get; set; } = string.Empty;
}
