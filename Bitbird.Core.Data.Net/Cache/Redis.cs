using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace Bitbird.Core.Data.Net
{
    public class DeferredRedisOperations
    {
        private readonly Redis redis;
        private readonly List<DeferredRedisDeleteOperation> deleted = new List<DeferredRedisDeleteOperation>();

        internal DeferredRedisOperations(Redis redis)
        {
            this.redis = redis;
        }

        internal void PushOperation(DeferredRedisDeleteOperation operation)
        {
            lock (deleted)
            {
                deleted.Add(operation);
            }
        }

        internal DeferredRedisDeleteOperation PopDeleteOperations()
        {
            DeferredRedisDeleteOperation[] data;
            lock (deleted)
            {
                data = deleted.ToArray();
                deleted.Clear();
            }

            return new DeferredRedisDeleteOperation(
                data.SelectMany(x => x.Keys)
                    .Distinct()
                    .ToArray());
        }

        public async Task ExecuteAsync()
        {
            var delete = PopDeleteOperations();
            await redis.DeleteManyAsync(delete.Keys);
        }
    }

    internal class DeferredRedisDeleteOperation
    {
        public readonly RedisKey[] Keys;

        public DeferredRedisDeleteOperation(params RedisKey[] keys)
        {
            Keys = keys;
        }
    }

    public class Redis : IDisposable
    {
        public static bool WriteDebugOutput = false;
        public static bool DeleteOnStartup = true;

        private readonly string connectionString;
        private readonly IContractResolver contractResolver;
        internal readonly ConnectionMultiplexer Connection;
        private bool isDisposed = false;

        public string FormatChannelForCurrentDb(string channel) => $"Db[{Db.Database}].{channel}";

        public static async Task<Redis> ConnectAsync(string connectionString, IContractResolver contractResolver = null)
        {
            var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            var redis = new Redis(connectionString, contractResolver, connection);

            if (DeleteOnStartup)
                await redis.ClearAsync();

            return redis;
        }

        public Redis(string connectionString, IContractResolver contractResolver, ConnectionMultiplexer connection)
        {
            this.connectionString = connectionString;
            this.contractResolver = contractResolver;
            Connection = connection;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (isDisposed)
                    throw new ObjectDisposedException(nameof(Redis));

                isDisposed = true;
                Connection.Dispose();
            }
        }

        public async Task<RedisCacheInfo> GetInfo()
        {
            var ret = new RedisCacheInfo
            {
                IsConnected = IsConnected
            };

            if (!ret.IsConnected)
                return ret;

            var result = await Db.ExecuteAsync("INFO", "memory");
            if (result.IsNull)
                return ret;

            var str = result.ToString();

            void ExtractValue(string name, Action<string> onValueFound)
            {
                var r = new Regex(Regex.Escape(name) + @":(\d*)?");
                var m = r.Match(str);
                if (m.Success && m.Groups.Count > 1)
                    onValueFound(m.Groups[1].Value);
            }

            // ReSharper disable once StringLiteralTypo
            ExtractValue("used_memory_dataset", value =>
            {
                if (long.TryParse(value, out var size))
                    ret.UsedMemory = size;
            });
            // ReSharper disable once StringLiteralTypo
            ExtractValue("maxmemory", value =>
            {
                if (long.TryParse(value, out var size))
                    ret.MaximumMemory = size;
            });

            return ret;
        }
        internal IDatabase Db => Connection.GetDatabase();
        private bool IsConnected => Connection.IsConnected;


        public DeferredRedisOperations StartDeferred()
        {
            return new DeferredRedisOperations(this);
        }

        private static RedisKey GetKey<TKey>(string prefix, TKey id)
            => $"{prefix}:{id?.ToString().ToLowerInvariant()}";

        private static RedisKey GetKey(string key)
            => key;

        public async Task<bool> ClearAsync()
        {
            if (!IsConnected)
                return false;

            if (WriteDebugOutput) Debug.WriteLine("RedisCache.Clear");

            // ReSharper disable once StringLiteralTypo
            await Db.ExecuteAsync("FLUSHDB");
            return true;
        }

        public async Task<long> DeleteAllAsync(string prefix)
        {
            var server = Connection.GetServer(connectionString.Split(',')[0]);
            if (!server.IsConnected)
                return -1;

            var keys = server.Keys(Db.Database, pattern: GetKey(prefix, "*").ToString()).ToArray();
            if (!IsConnected)
                return -1;

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.DeleteAll {prefix}");

            return await DeleteManyAsync(keys);
        }

        public Task<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            return DeleteAsync(GetKey(key));
        }
        public void DeferredDelete(DeferredRedisOperations deferredOperations, string key)
        {
            deferredOperations.PushOperation(new DeferredRedisDeleteOperation(GetKey(key)));
        }
        public Task<bool> DeleteAsync<TKey>(string prefix, TKey id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(prefix));

            return DeleteAsync(GetKey(prefix, id));
        }
        public void DeferredDelete<TKey>(DeferredRedisOperations deferredOperations, string prefix, TKey id)
        {
            deferredOperations.PushOperation(new DeferredRedisDeleteOperation(GetKey(prefix, id)));
        }

        public async Task<long> DeleteManyAsync<TKey>(string prefix, TKey[] ids)
        {
            if (!IsConnected)
                return 0;

            var keys = ids
                .Select(id => GetKey(prefix, id))
                .ToArray();

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.DeleteMany {string.Join(",", keys.Select(k => k.ToString()))}");

            return await DeleteManyAsync(keys);
        }
        public void DeferredDeleteMany<TKey>(DeferredRedisOperations deferredOperations, string prefix, TKey[] ids)
        {
            var keys = ids
                .Select(id => GetKey(prefix, id))
                .ToArray();

            deferredOperations.PushOperation(new DeferredRedisDeleteOperation(keys));
        }


        internal async Task<bool> DeleteAsync(RedisKey key)
        {
            if (!IsConnected) return false;
            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.Delete {key}");
            return await Db.KeyDeleteAsync(key);
        }
        internal async Task<long> DeleteManyAsync(RedisKey[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (!IsConnected) return 0;
            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.DeleteMany {string.Join(",", keys.Select(k => k.ToString()))}");
            return await Db.KeyDeleteAsync(keys);
        }


        private async Task<bool> SetAsync<T>(RedisKey key, T item, TimeSpan? expireTime)
        {
            if (!IsConnected)
                return false;

            var value = SerializeObject(item);

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.Set {key}: {value}");

            var result = await Db.StringSetAsync(key, value);

            if (expireTime.HasValue)
                await Db.KeyExpireAsync(key, expireTime);

            return result;
        }
        public Task<bool> SetAsync<TKey, T>(string prefix, TKey id, T item, TimeSpan? expireTime)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(prefix))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(prefix));

            return SetAsync(GetKey(prefix, id), item, expireTime);
        }
        public Task<bool> SetAsync<T>(string key, T item, TimeSpan? expireTime)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            return SetAsync(GetKey(key), item, expireTime);
        }

        private async Task<bool> SetManyAsync<T>(KeyValuePair<RedisKey, T>[] itemsById, TimeSpan? expireTime)
        {
            if (itemsById == null)
                throw new ArgumentNullException(nameof(itemsById));

            if (!IsConnected)
                return false;

            var data = itemsById
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(item.Key, SerializeObject(item.Value)))
                .ToArray();

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.SetMany {string.Join(",\n", data.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
            var result = await Db.StringSetAsync(data);

            if (expireTime.HasValue)
                await Task.WhenAll(
                    data
                        .Select(item => Db.KeyExpireAsync(item.Key, expireTime))
                        .ToArray());

            return result;
        }
        public Task<bool> SetManyAsync<TKey, T>(string prefix, IDictionary<TKey, T> itemsById, TimeSpan? expireTime)
        {
            if (itemsById == null)
                throw new ArgumentNullException(nameof(itemsById));

            var data = itemsById
                .Select(item => new KeyValuePair<RedisKey, T>(GetKey(prefix, item.Key), item.Value))
                .ToArray();

            return SetManyAsync(data, expireTime);
        }

        public Task AddOrUpdateAsync<TKey, T>(string prefix, TKey id, T item, TimeSpan? expireTime)
            => SetAsync(GetKey(prefix, id), item, expireTime);
        public Task AddOrUpdateAsync<T>(string key, T item, TimeSpan? expireTime)
            => SetAsync(GetKey(key), item, expireTime);
        public Task AddOrUpdateManyAsync<TKey, T>(string prefix, Dictionary<TKey, T> itemsById, TimeSpan? expireTime)
            => SetManyAsync(prefix, itemsById, expireTime);


        private async Task<(T value, bool exists)> GetAsync<T>(RedisKey key)
        {
            if (!IsConnected)
                return (default, false);

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.Get {key}");

            var value = await Db.StringGetAsync(key);
            if (value.HasValue)
            {
                try
                {
                    return (DeserializeObject<T>(value), true);
                }
                catch { /* ignored - cannot deserialize - must be refreshed */ }
            }

            return (default, false);
        }
        public Task<(T value, bool exists)> GetAsync<TKey, T>(string prefix, TKey id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return GetAsync<T>(GetKey(prefix, id));
        }
        public Task<(T value, bool exists)> GetAsync<T>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            return GetAsync<T>(GetKey(key));
        }
        private async Task<T> GetAsync<T>(RedisKey key, T valueIfNotExists)
        {
            var (value, exists) = await GetAsync<T>(key);
            return exists ? value : valueIfNotExists;
        }
        public Task<T> GetAsync<TKey, T>(string prefix, TKey id, T valueIfNotExists)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return GetAsync(GetKey(prefix, id), valueIfNotExists);
        }
        public Task<T> GetAsync<TKey, T>(string key, T valueIfNotExists)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            return GetAsync(GetKey(key), valueIfNotExists);
        }

        public async Task<T> GetOrAddAsync<T>(RedisKey key, Func<Task<T>> valueFactory, TimeSpan? expireTime)
        {
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!IsConnected)
                return await valueFactory();

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.GetOrAdd {key}");

            var (value, exists) = await GetAsync<T>(key);
            if (exists)
                return value;

            var newVal = await valueFactory();
            if (newVal != null)
                await SetAsync(key, newVal, expireTime);
            return newVal;
        }
        public Task<T> GetOrAddAsync<TKey, T>(string prefix, TKey id, Func<TKey, Task<T>> valueFactory, TimeSpan? expireTime)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            return GetOrAddAsync(GetKey(prefix, id), () => valueFactory(id), expireTime);
        }
        public Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? expireTime)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));

            return GetOrAddAsync(GetKey(key), valueFactory, expireTime);
        }
        public async Task<T[]> GetOrAddManyAsync<TKey, T>(string prefix, TKey[] ids, Func<TKey[], Task<T[]>> valueFactory, TimeSpan? expireTime)
        {
            if (ids == null)
                throw new ArgumentNullException(nameof(ids));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));
            
            if (!IsConnected)
                return await valueFactory(ids);

            var ret = new T[ids.Length];

            var keys = ids
                .Select(id => new KeyValuePair<TKey, RedisKey>(id, GetKey(prefix, id)))
                .ToArray();

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.GetOrAddMany {string.Join(",",keys.Select(k => k.Value.ToString()))}");

            var missing = new List<int>();
            var storedValues = await Db.StringGetAsync(keys.Select(key => key.Value).ToArray());

            for (var i = 0; i < storedValues.Length; i++)
            {
                var storedValue = storedValues[i];
                if (storedValue.HasValue)
                {
                    try
                    {
                        var element = DeserializeObject<T>(storedValue);

                        if (element != null && !element.Equals(default(T)))
                        {
                            ret[i] = element;
                            continue;
                        }
                    }
                    catch
                    { /* ignored - cannot deserialize - must be refreshed */ }
                }

                missing.Add(i);
            }

            if (missing.Any())
            {
                var missingKeys = missing.Select(idx => keys[idx].Key).ToArray();
                var missingElements = await valueFactory(missingKeys);

                foreach (var i in missing)
                    ret[i] = missingElements[i];

                await SetManyAsync(prefix, missingKeys
                    .Select((key, idx) => new
                    {
                        Key = key,
                        Value = missingElements[idx]
                    })
                    .GroupBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.First()), expireTime);
            }

            return ret;
        }


        public void CheckConnected()
        {
            if (!IsConnected)
                throw new Exception("Redis cache not connected");
        }

        public async Task LowLevelListAddAsync<T, TKey>(string prefix, TKey key, params T[] values)
        {
            if (values.Length == 0)
                return;

            var redisValues = values.Select(v => (RedisValue) SerializeObject(v)).ToArray();

            if (values.Length == 1)
                await Db.ListRightPushAsync(GetKey(prefix, key), redisValues[0], When.Always, CommandFlags.None);
            else
                await Db.ListRightPushAsync(GetKey(prefix, key), redisValues, CommandFlags.None);
        }
        public async Task LowLevelListRemoveAsync<T, TKey>(string prefix, TKey key, params T[] values)
        {
            if (values.Length == 0)
                return;
            
            foreach (var value in values)
                await Db.ListRemoveAsync(GetKey(prefix, key), SerializeObject(value), 0, CommandFlags.None);
        }
        public async Task LowLevelListClearAsync<TKey>(string prefix, TKey key)
        {
            await Db.KeyDeleteAsync(GetKey(prefix, key), CommandFlags.None);
        }
        public async Task<T[]> LowLevelListGetAsync<T, TKey>(string prefix, TKey key)
        {
            return (await Db.ListRangeAsync(GetKey(prefix, key))).Select(x => DeserializeObject<T>(x)).ToArray();
        }

        public async Task LowLevelSetAddAsync<T, TKey>(string prefix, TKey key, params T[] values)
        {
            if (values.Length == 0)
                return;

            var redisValues = values.Select(v => (RedisValue)SerializeObject(v)).ToArray();
            await Db.SetAddAsync(GetKey(prefix, key), redisValues);
        }
        public async Task LowLevelSetRemoveAsync<T, TKey>(string prefix, TKey key, params T[] values)
        {
            if (values.Length == 0)
                return;

            var redisValues = values.Select(v => (RedisValue)SerializeObject(v)).ToArray();
            await Db.SetRemoveAsync(GetKey(prefix, key), redisValues);
        }
        public async Task LowLevelSetClearAsync<TKey>(string prefix, TKey key)
        {
            await Db.KeyDeleteAsync(GetKey(prefix, key));
        }
        public async Task<T[]> LowLevelSetGetAsync<T, TKey>(string prefix, TKey key)
        {
            return (await Db.SetMembersAsync(GetKey(prefix, key))).Select(x => DeserializeObject<T>(x)).ToArray();
        }



        private JsonSerializerSettings serializerSettings;

        private JsonSerializerSettings SerializerSettings =>
            serializerSettings ?? (serializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = contractResolver
            });
        internal string SerializeObject(object objectToCache)
        {
            return JsonConvert.SerializeObject(objectToCache
                , Formatting.Indented
                , SerializerSettings);
        }
        internal T DeserializeObject<T>(string serializedObject)
        {
            return JsonConvert.DeserializeObject<T>(serializedObject
                , SerializerSettings);
        }
    }
}
