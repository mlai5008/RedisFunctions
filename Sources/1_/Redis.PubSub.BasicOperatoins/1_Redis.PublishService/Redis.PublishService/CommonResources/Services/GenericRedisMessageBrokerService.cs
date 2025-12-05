using CommonResources.CacheOptions;
using CommonResources.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace CommonResources.Services
{
    public class GenericRedisMessageBrokerService<T> : IGenericRedisMessageBrokerService<T> where T : class
    {
        private readonly ISubscriber _subscriber;
        private readonly RedisCacheOptions _redisCacheOptions;
        private readonly ILogger<GenericRedisMessageBrokerService<T>> _logger;

        public GenericRedisMessageBrokerService(ILogger<GenericRedisMessageBrokerService<T>> logger, IConnectionMultiplexer connectionMultiplexer, IOptions<RedisCacheOptions> options)
        {
            _subscriber = connectionMultiplexer.GetSubscriber();
            _redisCacheOptions = options.Value;
            _logger = logger;
        }

        public async Task<long> PublishAsync(T value)
        {
            var message = JsonSerializer.Serialize<T>(value);
            var number = await _subscriber.PublishAsync(RedisChannel.Literal(_redisCacheOptions.InstanceName), message);
            _logger.LogInformation($"-//- {typeof(GenericRedisMessageBrokerService<>).Name}. Item [{message}] published. {DateTimeOffset.Now}");
            return number;
        }

        public async Task SubscribeAsync(Action<RedisChannel, RedisValue> handler)
        {
            await _subscriber.SubscribeAsync(RedisChannel.Literal(_redisCacheOptions.InstanceName), handler);
        }
    }
}
