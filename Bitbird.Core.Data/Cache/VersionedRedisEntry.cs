using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Bitbird.Core.Data.Cache
{
    public class VersionedRedisEntry<T>
        : IEquatable<VersionedRedisEntry<T>>
            , IComparable<VersionedRedisEntry<T>>
    {
        [JsonProperty(
            PropertyName = "d",
            DefaultValueHandling = DefaultValueHandling.Include,
            Required = Required.AllowNull)]
        public T Data { get; }

        [JsonProperty(
            PropertyName = "v", 
            DefaultValueHandling = DefaultValueHandling.Include, 
            Required = Required.DisallowNull)]
        public uint Version { get; }

        [JsonConstructor]
        public VersionedRedisEntry(T d, uint v)
        {
            Data = d;
            Version = v;
        }

        public void Deconstruct(out T data, out uint version)
        {
            data = Data;
            version = Version;
        }

        public bool Equals(VersionedRedisEntry<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<T>.Default.Equals(Data, other.Data) && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VersionedRedisEntry<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Data) * 397) ^ (int) Version;
            }
        }

        public int CompareTo(VersionedRedisEntry<T> other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var comp = Version.CompareTo(other.Version);
            if (comp != 0) return comp;

            if (ReferenceEquals(Data, other.Data)) return 0;
            if (ReferenceEquals(null, Data)) return 1;
            if (ReferenceEquals(null, other.Data)) return -1;

            if (!(Data is IComparable<T> comparableData))
                return 0;

            return comparableData.CompareTo(other.Data);
        }


        public override string ToString() => $"v{Version} {{ {Data} }}";
    }
}