using SistemaLivro.Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace SistemaLivro.Infrastructure.Redis;

public class RedisService(IDistributedCache cache) : IRedisService
{
    public async Task SetValueAsync(string key, string value)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        await cache.SetStringAsync(key, value, options);
    }

    public async Task<string?> GetValueAsync(string key)
    {
        return await cache.GetStringAsync(key);
    }

    public async Task RemoveValueAsync(string key)
    {
        await cache.RemoveAsync(key);
    }
}