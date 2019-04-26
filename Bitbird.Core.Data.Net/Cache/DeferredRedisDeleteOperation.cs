using StackExchange.Redis;

namespace Bitbird.Core.Data.Net.Cache
{
    internal class DeferredRedisDeleteOperation
    {
        public readonly RedisKey[] Keys;

        public DeferredRedisDeleteOperation(params RedisKey[] keys)
        {
            Keys = keys;
        }
    }
}