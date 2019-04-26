using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bitbird.Core.Data.Net.Cache
{
    /// <summary>
    /// Defines a singleton <see cref="DefaultContractResolver" /> that is used for serialization and deserialization to/from the redis cache.
    /// </summary>
    /// <inheritdoc cref="DefaultContractResolver" />
    public class SimpleCacheContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        [UsedImplicitly]
        public static readonly SimpleCacheContractResolver Instance = new SimpleCacheContractResolver();

        /// <inheritdoc />
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);
            foreach (var prop in props)
            {
                prop.DefaultValueHandling = DefaultValueHandling.Ignore;
                prop.NullValueHandling = NullValueHandling.Ignore;
                prop.Converter = null;  // Ignore [JsonConverter]
                prop.PropertyName = prop.UnderlyingName;  // restore original property name
            }
            return props;
        }
    }
}