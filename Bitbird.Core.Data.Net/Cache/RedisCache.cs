using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bitbird.Core.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace Bitbird.Core.Data.Net
{
    public class RedisCache
    {
        private readonly string connectionString;
        private readonly IContractResolver contractResolver;
        private readonly Lazy<ConnectionMultiplexer> lazyConnection;

        public static bool WriteDebugOutput = false;

        public RedisCache(string connectionString, IContractResolver contractResolver = null)
        {
            this.connectionString = connectionString;
            this.contractResolver = contractResolver;
            lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(this.connectionString));

            AsyncHelper.RunSync(async () => await ClearAsync());
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

        private ConnectionMultiplexer Connection 
            => lazyConnection.Value;
        private IDatabase Db 
            => Connection.GetDatabase();
        private bool IsConnected 
            => Connection.IsConnected;

        public async Task<bool> ClearAsync()
        {
            if (!IsConnected)
                return false;

            if (WriteDebugOutput) Debug.WriteLine("RedisCache.Clear");
            // ReSharper disable once StringLiteralTypo
            await Db.ExecuteAsync("FLUSHALL");
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

            return await Db.KeyDeleteAsync(keys);
        }
        public async Task<bool> DeleteAsync<TKey>(string prefix, TKey id)
        {
            if (!IsConnected)
                return false;

            var key = GetKey(prefix, id);

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.Delete {key}");

            return !(await Db.KeyExistsAsync(key)) || await Db.KeyDeleteAsync(key);
        }
        public async Task<long> DeleteManyAsync<TKey>(string prefix, TKey[] ids)
        {
            if (!IsConnected)
                return 0;

            var keys = ids
                .Select(id => GetKey(prefix, id))
                .ToArray();

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.DeleteMany {string.Join(",", keys.Select(k => k.ToString()))}");

            return await Db.KeyDeleteAsync(keys);
        }

        private static RedisKey GetKey<TKey>(string prefix, TKey id) 
            => $"{prefix}:{id}";


        private async Task<bool> SetAsync<T>(RedisKey key, T item)
        {
            if (!IsConnected)
                return false;

            var value = SerializeObject(item);

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.Set {key}: {value}");

            return await Db.StringSetAsync(key, value);
        }
        private async Task<bool> SetAsync<TKey, T>(string prefix, TKey id, T item)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var key = GetKey(prefix, id);
            return await SetAsync(key, item);
        }
        private async Task<bool> SetManyAsync<TKey, T>(string prefix, IDictionary<TKey, T> itemsById)
        {
            if (itemsById == null)
                throw new ArgumentNullException(nameof(itemsById));

            if (!IsConnected)
                return false;

            var data = itemsById
                .Select(item => new KeyValuePair<RedisKey, RedisValue>(GetKey(prefix, item.Key), SerializeObject(item.Value)))
                .ToArray();

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.SetMany {string.Join(",\n", data.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
            return await Db.StringSetAsync(data);
        }

        public Task AddOrUpdateAsync<TKey, T>(string prefix, TKey id, T item)
            => SetAsync(prefix, id, item);
        public Task AddOrUpdateManyAsync<TKey, T>(string prefix, Dictionary<TKey, T> itemsById)
            => SetManyAsync(prefix, itemsById);

        public async Task<T> GetOrAddAsync<TKey, T>(string prefix, TKey id, Func<TKey, Task<T>> valueFactory)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!IsConnected)
                return await valueFactory(id);

            var key = GetKey(prefix, id);

            if (WriteDebugOutput) Debug.WriteLine($"RedisCache.GetOrAdd {key}");

            var value = await Db.StringGetAsync(key);
            if (value.HasValue)
            { 
                try
                {
                    return DeserializeObject<T>(value);
                }
                catch { /* ignored - cannot deserialize - must be refreshed */ }
            }

            var newVal = await valueFactory(id);
            if (newVal != null)
                await SetAsync(prefix, id, newVal);
            return newVal;
        }
        public async Task<T[]> GetOrAddManyAsync<TKey, T>(string prefix, TKey[] ids, Func<TKey[], Task<T[]>> valueFactory)
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
                    .ToDictionary(x => x.Key, x => x.First()));
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
        private string SerializeObject(object objectToCache)
        {
            return JsonConvert.SerializeObject(objectToCache
                , Formatting.Indented
                , SerializerSettings);
        }
        private T DeserializeObject<T>(string serializedObject)
        {
            return JsonConvert.DeserializeObject<T>(serializedObject
                , SerializerSettings);
        }
    }
}
