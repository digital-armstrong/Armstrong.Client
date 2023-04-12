using StackExchange.Redis;

namespace Armstrong.Client.Helpers
{
    public class RedisHelper
    {
        public ConnectionMultiplexer RedisConnMultiplexer { get; set; }
        public ISubscriber RedisSubscriber { get; set; }

        public RedisHelper(string host)
        {
            RedisConnMultiplexer = ConnectionMultiplexer.Connect(host);
            RedisSubscriber = RedisConnMultiplexer.GetSubscriber();
        }
    }
}
