using NotificationService.Core.Interfaces;
using StackExchange.Redis;

namespace NotificationService.Infrastructure.Providers;

public class RedisRateLimiter : IRateLimiter
{
    private readonly IConnectionMultiplexer _redis;

    public RedisRateLimiter(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
    {
        var db = _redis.GetDatabase();
        
        // Atomic increment
        var count = await db.StringIncrementAsync(key);

        if (count == 1)
        {
            // If it's the first request in the window, set the expiration
            await db.KeyExpireAsync(key, window);
        }

        return count <= limit;
    }
}
