using NotificationService.Core.Enums;

namespace NotificationService.Core.Entities;

public class NotificationRecord
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public NotificationStatus Status { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string? ProviderResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
