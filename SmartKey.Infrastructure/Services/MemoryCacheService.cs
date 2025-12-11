using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using SmartKey.Application.Common.Cache;
using SmartKey.Application.Common.Interfaces.Services;

namespace SmartKey.Infrastructure.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly string _prefix;
        private readonly TimeSpan _absTtl;
        private readonly TimeSpan _sldTtl;

        public MemoryCacheService(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;

            _prefix = _configuration["Cache:KeyPrefix"] ?? "WorkFlowApp:";
            _absTtl = TimeSpan.FromMinutes(int.TryParse(_configuration["Cache:DefaultAbsoluteTtlMinutes"], out var abs) ? abs : 60);
            _sldTtl = TimeSpan.FromMinutes(int.TryParse(_configuration["Cache:DefaultSlidingTtlMinutes"], out var sld) ? sld : 30);
        }

        private string FormatKey(string key) => $"{_prefix}{key}";

        public Task SetAsync<T>(T model, TimeSpan? absoluteTtl = null, TimeSpan? slidingTtl = null)
            where T : CacheModelBase
        {
            var cacheKey = FormatKey(model.GetCacheKey());

            var abs = model.AbsoluteExpiresAt?.Subtract(DateTime.UtcNow)
                      ?? absoluteTtl ?? _absTtl;
            var sld = model.SlidingExpiresAt?.Subtract(DateTime.UtcNow)
                        ?? slidingTtl ?? _sldTtl;

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = abs,
                SlidingExpiration = sld
            };

            _cache.Set(FormatKey(cacheKey), model, options);
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string cacheKey)
            where T : CacheModelBase
        {
            if (_cache.TryGetValue(FormatKey(cacheKey), out T? model))
            {
                return Task.FromResult(model);
            }
            return Task.FromResult<T?>(default);
        }
        public Task RemoveAsync(string cacheKey)
        {
            _cache.Remove(FormatKey(cacheKey));
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string cacheKey)
        {
            var exists = _cache.TryGetValue(FormatKey(cacheKey), out _);
            return Task.FromResult(exists);
        }
    }
}