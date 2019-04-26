using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Net.Cache
{
    public class CacheById<TId, TValue>
    {
        [CanBeNull] private readonly Redis cache;
        [NotNull] private readonly string prefix;
        [CanBeNull] private readonly TimeSpan? expiration;

        public CacheById(
            [CanBeNull] Redis cache, 
            [NotNull] string prefix,
            bool expire)
            : this(cache, prefix, CacheHelper.GetExpiration(expire))
        {
        }

        public CacheById(
            [CanBeNull] Redis cache,
            [NotNull] string prefix,
            [CanBeNull] TimeSpan? expiration)
        {
            this.cache = cache;
            this.prefix = prefix;
            this.expiration = expiration;
        }

        [NotNull, ItemCanBeNull]
        public async Task<TValue> GetOrAddAsync(
            [NotNull] TId id, 
            [NotNull] Func<TId, Task<TValue>> addTask)
        {
            if (cache == null)
                return await addTask(id);

            return await cache.GetOrAddAsync(prefix, id, addTask, expiration);
        }

        [NotNull, ItemNotNull]
        public async Task<TValue[]> GetOrAddManyAsync(
            [NotNull, ItemNotNull] TId[] ids,
            [NotNull] Func<TId[], Task<TValue[]>> addTask)
        {
            if (cache == null)
                return await addTask(ids);

            return await cache.GetOrAddManyAsync(prefix, ids, addTask, expiration);
        }

        [NotNull]
        public Task<(TValue value, bool exists)> GetAsync([NotNull] TId id)
        {
            if (cache == null) throw new Exception("Accessing cached value, and no cache is set-up.");
            return cache.GetAsync<TId, TValue>(prefix, id);
        }

        [NotNull]
        public Task SetAsync([NotNull] TId id, [NotNull] TValue data)
        {
            if (cache == null) throw new Exception("Setting cached value, and no cache is set-up.");
            return cache.SetAsync(prefix, id, data, expiration);
        }

        [NotNull]
        public async Task AddOrUpdateAsync([NotNull] TId id, TValue value)
        {
            if (cache != null)
                await cache.AddOrUpdateAsync(prefix, id, value, expiration);
        }

        [NotNull]
        public async Task AddOrUpdateManyAsync([NotNull] Dictionary<TId, TValue> data)
        {
            if (cache != null)
                await cache.AddOrUpdateManyAsync(prefix, data, expiration);
        }

        public void DeferredDelete([NotNull] DeferredRedisOperations deferredOperations, [NotNull, ItemNotNull] params TId[] ids)
        {
            cache?.DeferredDeleteMany(deferredOperations, prefix, ids);
        }
    }
}
