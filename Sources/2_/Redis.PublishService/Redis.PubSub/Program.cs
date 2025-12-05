using StackExchange.Redis;
using System.Text.Json;

namespace Redis.PubSub
{
    internal class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Press <Enter> to exit...");
            Console.WriteLine("Press <P> to publish message.");
            const string configuration = "localhost:6379,abortConnect=false,ssl=true";
            const string channelName = "RedisChannel";
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(configuration);
            var subscriber = connectionMultiplexer.GetSubscriber();

            while (true)
            {
                var keiKey = Console.ReadKey().Key;
                if (keiKey == ConsoleKey.P)
                {
                    (Guid, string) message = new(Guid.NewGuid(), DateTime.UtcNow.ToString());
                    var json = JsonSerializer.Serialize(message);
                    await subscriber.PublishAsync(RedisChannel.Literal(channelName), json);
                    Console.WriteLine($"{message.Item1} {message.Item2}");
                }
                if (keiKey == ConsoleKey.Enter)
                {
                    break;
                }
            }

            Console.WriteLine("EXIT!");
        }
    }
}
