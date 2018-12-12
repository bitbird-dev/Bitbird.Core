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
        [JsonIgnore, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// Method can be used to restrict Access to certain properties in the Bitbird.Core.JsonApiDocument.
        /// Will be called for every property tagged with a JsonAccessRestrictedAttribute.
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public virtual bool IsPropertyAccessible(PropertyInfo propertyInfo)
        {
            return true;
        }

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
