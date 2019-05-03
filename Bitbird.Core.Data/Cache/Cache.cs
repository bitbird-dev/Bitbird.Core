using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Cache
{
    public class Cache<TValue>
    {
        [CanBeNull] private readonly Redis cache;
        [NotNull] private readonly string key;
        [CanBeNull] private readonly TimeSpan? expiration;
        
        public Cache(
            [CanBeNull] Redis cache,
            [NotNull] string key,
            bool expire)
            : this(cache, key, CacheHelper.GetExpiration(expire))
        {
        }
        public Cache(
            [CanBeNull] Redis cache,
            [NotNull] string key,
            [CanBeNull] TimeSpan? expiration)
        {
            this.cache = cache;
            this.key = key;
            this.expiration = expiration;
        }

        [NotNull, ItemCanBeNull]
        public async Task<TValue> GetOrSetAsync([NotNull] Func<Task<TValue>> addTask)
        {
            if (cache == null)
                return await addTask();

            return await cache.GetOrAddAsync(key, addTask, expiration);
        }

        [NotNull]
        public Task<(TValue value, bool exists)> GetAsync()
        {
            if (cache == null) throw new Exception("Accessing cached value directly while no cache is set-up.");
            return cache.GetAsync<TValue>(key);
        }

        [NotNull]
        public Task SetAsync([NotNull] TValue data)
        {
            if (cache == null) throw new Exception("Setting cached value directly while no cache is set-up.");
            return cache.SetAsync(key, data, expiration);
        }

        public void DeferredClear([NotNull] DeferredRedisOperations deferredOperations)
        {
            cache?.DeferredDelete(deferredOperations, key);
        }
    }
}