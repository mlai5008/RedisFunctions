using StackExchange.Redis;

namespace CommonResources.Services.Interfaces
{
    public interface IGenericRedisMessageBrokerService<T> where T : class
    {
        Task<long> PublishAsync(T message);
        Task SubscribeAsync(Action<RedisChannel, RedisValue> handler);
    }
}
