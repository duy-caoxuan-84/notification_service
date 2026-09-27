namespace NotificationService.Core.Interfaces;

public interface IRateLimiter
{
    Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window);
}
