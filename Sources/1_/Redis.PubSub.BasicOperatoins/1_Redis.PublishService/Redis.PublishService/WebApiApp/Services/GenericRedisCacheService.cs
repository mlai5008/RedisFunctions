using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebApiApp.Services.Interfaces;

namespace WebApiApp.Services
{
    public class GenericRedisCacheService<T> : IGenericRedisCacheService<T> where T : class
    {
        private readonly ILogger<GenericRedisCacheService<T>> _logger;
        private readonly IDistributedCache _redisCache;

        public GenericRedisCacheService(ILogger<GenericRedisCacheService<T>> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _redisCache = distributedCache;           
        }

        public async Task<T?> GetValueAsync<T>(string key)
        {            
            var stringValue = await _redisCache.GetStringAsync(key);
            if (stringValue != null)
            {
                var value = JsonSerializer.Deserialize<T>(stringValue);
                if (value != null)
                {
                    _logger.LogInformation($"-/- {typeof(GenericRedisCacheService<>).Name}. Item [{stringValue}] find to cache. {DateTimeOffset.Now}");
                    return value;
                }
            }
            _logger.LogInformation($"-/- {typeof(GenericRedisCacheService<>).Name}. Item don't find to cache. {DateTimeOffset.Now}");
            return default;
        }

        public async Task SetValueAsync<T>(string key, T value)
        {
            var stringValue = JsonSerializer.Serialize<T>(value);
            await _redisCache.SetStringAsync(key, stringValue, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });
            _logger.LogInformation($"-/- {typeof(GenericRedisCacheService<>).Name}. Item [{stringValue}] add to cache. {DateTimeOffset.Now}");
        }
    }
}
