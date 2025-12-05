using CommonResources.Services;
using StackExchange.Redis;
using System.Text.Json;
using WebApiApp.Services.Interfaces;

namespace WebApiApp.Services
{
    public class GenericHandlerService<T> : IGenericHandlerService<T> where T : class
    {
        private readonly ILogger<GenericHandlerService<T>> _logger;

        public GenericHandlerService(ILogger<GenericHandlerService<T>> logger)
        {
            _logger = logger;
        }

        public void HandlerMessage(RedisChannel redisChannel, RedisValue value)
        {
            var message = JsonSerializer.Deserialize<T>(value);
            _logger.LogInformation($"-//- {typeof(GenericRedisMessageBrokerService<>).Name}. Item [{message}] subscribed. {DateTimeOffset.Now}");
        }
    }
}