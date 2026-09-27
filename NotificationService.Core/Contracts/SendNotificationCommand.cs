using NotificationService.Core.Enums;

namespace NotificationService.Core.Contracts;

public class SendNotificationCommand
{
    public Guid NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public NotificationChannel Channel { get; set; }
    public string Payload { get; set; } = string.Empty;
}
