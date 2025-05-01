using Microsoft.AspNetCore.OutputCaching;
using StackExchange.Redis;

namespace DesafioCCAA.Infrastructure.Redis;

public class RedisOutputCacheStore : IOutputCacheStore
{
    private readonly IDatabase _database;

    public RedisOutputCacheStore(IDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
    }

    public async ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(tag))
            throw new ArgumentNullException(nameof(tag));

        var tagKey = $"tag:{tag}";
        var keys = await _database.SetMembersAsync(tagKey);

        if (keys.Length > 0)
        {
            var redisKeys = keys.Select(k => (RedisKey)k.ToString()).ToArray();

            await _database.KeyDeleteAsync(redisKeys);
        }

        await _database.KeyDeleteAsync(tagKey);
    }

    public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var value = await _database.StringGetAsync(key);
        return value.HasValue ? (byte[]?)value : null;
    }

    public async ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (value == null || value.Length == 0)
            throw new ArgumentNullException(nameof(value));

        await _database.StringSetAsync(key, value, validFor);

        if (tags != null)
        {
            foreach (var tag in tags)
            {
                var tagKey = $"tag:{tag}";
                await _database.SetAddAsync(tagKey, key);
                await _database.KeyExpireAsync(tagKey, validFor);
            }
        }
    }
}