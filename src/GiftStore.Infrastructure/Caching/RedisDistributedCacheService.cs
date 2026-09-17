namespace GiftStore.Infrastructure.Caching;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using GiftStore.Application.Interfaces;

public class RedisDistributedCacheService : IDistributedCacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RedisDistributedCacheService> _logger;

    public RedisDistributedCacheService(IConnectionMultiplexer? redis, ILogger<RedisDistributedCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return default;
        try
        {
            var db = _redis.GetDatabase();
            var val = await db.StringGetAsync(key);
            if (!val.HasValue) return default;
            return JsonSerializer.Deserialize<T>(val.ToString()!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET failure for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return;
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, json, expiry ?? TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET failure for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_redis == null || !_redis.IsConnected) return;
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis DEL failure for key {Key}", key);
        }
    }
}
