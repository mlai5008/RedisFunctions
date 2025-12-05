using StackExchange.Redis;

namespace WebApiApp.Services.Interfaces
{
    public interface IGenericHandlerService<T> where T : class
    {
        void HandlerMessage(RedisChannel redisChannel, RedisValue value);
    }
}
