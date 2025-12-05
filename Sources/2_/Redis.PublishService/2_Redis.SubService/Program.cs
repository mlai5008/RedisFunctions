using StackExchange.Redis;
using System.Text.Json;

namespace _2_Redis.SubService
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string configuration = "localhost:6379,abortConnect=false,ssl=true";
            const string channelName = "RedisChannel";
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
            var subscriber = connectionMultiplexer.GetSubscriber();

            await subscriber.SubscribeAsync(RedisChannel.Literal(channelName), (channel, json) =>
            {
                var message = JsonSerializer.Deserialize<(Guid, DateTime)>(json);
                Console.WriteLine($"{message.Item1} {message.Item2}");
            });

            Console.WriteLine("Press key to exit...");
            Console.ReadLine();
        }
    }
}
