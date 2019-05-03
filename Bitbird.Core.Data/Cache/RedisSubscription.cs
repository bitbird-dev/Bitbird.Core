using System;
using System.Threading.Tasks;
using Bitbird.Core.Tasks;
using StackExchange.Redis;

namespace Bitbird.Core.Data.Cache
{
    public class RedisSubscription<TMessage> : IDisposable
    {
        private bool isSubscribed = false;
        private readonly Redis redis;
        private readonly RedisChannel channel;

        public event Action<TMessage> Message;

        public RedisSubscription(Redis redis, string channel)
        {
            this.redis = redis;
            this.channel = new RedisChannel(redis.FormatChannelForCurrentDb(channel), RedisChannel.PatternMode.Literal);
        }

        public async Task SubscribeAsync()
        {
            lock (this)
            {
                if (isSubscribed)
                    throw new Exception($"{nameof(RedisSubscription<TMessage>)} is already subscribed.");

                isSubscribed = true;
            }
            await redis.Connection.GetSubscriber().SubscribeAsync(channel, OnMessage);
        }

        public async Task UnsubscribeAsync()
        {
            lock (this)
            {
                if (!isSubscribed)
                    return;

                isSubscribed = false;
            }
            await redis.Connection.GetSubscriber().UnsubscribeAsync(channel, OnMessage);
        }

        private void OnMessage(RedisChannel channel, RedisValue value)
        {
            try
            {
                var message = redis.DeserializeObject<TMessage>(value);
                Message?.Invoke(message);
            }
            catch
            {
                /* ignored */
                // TODO: log
            }
        }

        public void Dispose()
            => AsyncHelper.RunSync(UnsubscribeAsync);
    }
}