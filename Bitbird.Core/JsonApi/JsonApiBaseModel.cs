using Bitbird.Core.JsonApi.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    [JsonObject(NamingStrategyType = typeof(Newtonsoft.Json.Serialization.CamelCaseNamingStrategy))]
    public abstract class JsonApiBaseModel
    {
        [JsonIgnore]
        public string Id { get; set; }
        
        public string GetJsonApiClassName()
        {
            return GetJsonApiClassName(GetType());
        }

        public static string GetJsonApiClassName(Type type)
        {
            var customName = type.GetTypeInfo().GetCustomAttribute<JsonApiClassAttribute>();
            string typeName = (customName != null) ? customName.Name : type.Name;
            typeName = typeName.Trim();
            return typeName.ToLowerInvariant();
        }
    }
}
