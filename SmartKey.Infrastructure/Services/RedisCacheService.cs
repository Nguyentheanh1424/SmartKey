using Microsoft.Extensions.Configuration;
using SmartKey.Application.Common.Cache;
using SmartKey.Application.Common.Interfaces.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace SmartKey.Infrastructure.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _redis;
        private readonly IConfiguration _configuration;
        private readonly string _prefix;
        private readonly TimeSpan _absTtl;

        public RedisCacheService(IConnectionMultiplexer connection, IConfiguration configuration)
        {
            _configuration = configuration;
            _redis = connection.GetDatabase(int.TryParse(_configuration["Cache:Database"], out var db) ? db : 0);
            _prefix = _configuration["Cache:KeyPrefix"] ?? "WorkFlowApp:";
            _absTtl = TimeSpan.FromMinutes(int.TryParse(_configuration["Cache:DefaultAbsoluteTtlMinutes"], out var abs) ? abs : 60);
        }

        private string FormatKey(string cacheKey) => $"{_prefix}{cacheKey}";

        public Task SetAsync<T>(T model, TimeSpan? absoluteTtl = null, TimeSpan? slidingTtl = null) where T : CacheModelBase
        {
            var cacheKey = FormatKey(model.GetCacheKey());
            var json = JsonSerializer.Serialize(model);

            var ttl = absoluteTtl ?? slidingTtl ?? _absTtl;

            return _redis.StringSetAsync(cacheKey, json, ttl);
        }

        public async Task<T?> GetAsync<T>(string cacheKey) where T : CacheModelBase
        {
            var json = await _redis.StringGetAsync(FormatKey(cacheKey));
            if (!json.HasValue)
                return default;

            var model = JsonSerializer.Deserialize<T>(json!)!;
            if (model == null)
                return default;

            var isExpired = model.AbsoluteExpiresAt.HasValue && DateTime.UtcNow >= model.AbsoluteExpiresAt.Value;
            if (isExpired)
            {
                await RemoveAsync(cacheKey);
                return default;
            }

            if (model.SlidingExpiresAt.HasValue)
            {
                var slidingTtl = model.SlidingExpiresAt.Value - DateTime.UtcNow;
                if (slidingTtl > TimeSpan.Zero)
                {
                    model.RefreshSlidingExpiration(slidingTtl);
                    await _redis.StringSetAsync(FormatKey(cacheKey), JsonSerializer.Serialize(model), slidingTtl);
                }
            }
            return model;
        }

        public Task RemoveAsync(string cacheKey)
        {
            return _redis.KeyDeleteAsync(FormatKey(cacheKey));
        }

        public async Task<bool> ExistsAsync(string cacheKey)
        {
            return await _redis.KeyExistsAsync(FormatKey(cacheKey));
        }
    }
}
