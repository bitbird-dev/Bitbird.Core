using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi.ResourceObjectDictionary
{
    [JsonConverter(typeof(JsonApiResourceObjectDictionaryConverter))]
    public class JsonApiResourceObjectDictionary
    {
        public Dictionary<ResourceKey, JsonApiResourceObject> ResourceObjectDictionary { get; } = new Dictionary<ResourceKey, JsonApiResourceObject>();

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonApiResourceObjectDictionary()
        {

        }

        /// <summary>
        /// Adds a resource to the Dictionary.
        /// Ignores duplicates.
        /// </summary>
        /// <param name="resource"></param>
        public void AddResource(JsonApiResourceObject resource)
        {
            var key = new ResourceKey(resource.Id, resource.Type);
            if (ResourceObjectDictionary.ContainsKey(key)) { return; }
            ResourceObjectDictionary.Add(key, resource);
        }

        /// <summary>
        /// Retrievews a resource from the Dictionary.
        /// Returns null if resource does not exist.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public JsonApiResourceObject GetResource(string resourceId, string resourceType)
        {
            var key = new ResourceKey(resourceId, resourceType);
            JsonApiResourceObject result = null;
            ResourceObjectDictionary.TryGetValue(key, out result);
            return result;
        }
    }
}
