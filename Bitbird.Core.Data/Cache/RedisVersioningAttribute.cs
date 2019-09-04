using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;

namespace Bitbird.Core.Data.Cache
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RedisVersioningAttribute : Attribute
    {
        public uint Version { get; }

        public RedisVersioningAttribute(uint version = 0u)
        {
            Version = version;
        }

        public override string ToString() => $"v{Version}";


        private static readonly ConcurrentDictionary<Type, uint> VersionsByType = new ConcurrentDictionary<Type, uint>();
        public static uint GetVersion([NotNull] Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            return VersionsByType.GetOrAdd(t, type => type.GetCustomAttribute<RedisVersioningAttribute>()?.Version ?? 0u);
        }
        public static uint GetVersion<T>() => GetVersion(typeof(T));
    }
}