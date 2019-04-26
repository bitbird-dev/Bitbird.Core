using System.Threading.Tasks;
using StackExchange.Redis;

namespace Bitbird.Core.Data.Net.Cache
{
    public class RedisPublisher<TMessage>
    {
        private readonly Redis redis;
        private readonly RedisChannel channel;

        public RedisPublisher(Redis redis, string channel)
            : this(redis, new RedisChannel(redis.FormatChannelForCurrentDb(channel), RedisChannel.PatternMode.Literal))
        { }
        public RedisPublisher(Redis redis, RedisChannel channel)
        {
            this.redis = redis;
            this.channel = channel;
        }

        public Task PublishAsync(TMessage message)
        {
            var redisValue = redis.SerializeObject(message);
            return redis.Connection.GetSubscriber().PublishAsync(channel, redisValue);
        }
    }
}