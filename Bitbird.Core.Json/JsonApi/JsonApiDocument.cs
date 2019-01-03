using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.JsonApi.Dictionaries;
using Bitbird.Core.Json.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.JsonApi
{
    public class JsonDocumentDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonApiResourceObject).IsAssignableFrom(objectType) || typeof(IEnumerable<JsonApiResourceObject>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType != JsonToken.StartArray)
            {
                var res = serializer.Deserialize<JsonApiResourceObject>(reader);
                if (res != null)
                {
                    return new List<JsonApiResourceObject> { res };
                }
            }
            return serializer.Deserialize(reader, objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resourceCollection = value as IEnumerable<JsonApiResourceObject>;
            if(resourceCollection != null)
            {
                var count = resourceCollection.Count();
                
                if (count == 1)
                {
                    var singleResource = resourceCollection.FirstOrDefault();
                    serializer.Serialize(writer, singleResource);
                }
                else
                {
                    serializer.Serialize(writer, resourceCollection);
                }
            }
            else
            {
                writer.WriteNull();
            }
        }
    }

    /// <summary>
    /// A document MUST contain at least one of the following top-level members:
    ///
    ///     data: the document’s “primary data”
    ///     errors: an array of error objects
    ///     meta: a meta object that contains non-standard meta-information.
    ///     
    /// The members data and errors MUST NOT coexist in the same document.
    /// 
    /// A document MAY contain any of these top-level members:
    /// 
    ///     jsonapi: an object describing the server’s implementation
    ///     links: a links object related to the primary data.
    ///     included: an array of resource objects that are related to the primary data and/or each other (“included resources”).
    /// 
    /// If a document does not contain a top-level data key, the included member MUST NOT be present either.
    /// 
    /// The top-level links object MAY contain the following members:
    ///     
    ///     self: the link that generated the current response document.
    ///     related: a related resource link when the primary data represents a resource relationship.
    ///     pagination links for the primary data.
    ///     
    /// Primary data MUST be either:
    /// 
    ///     a single resource object, a single resource identifier object, or null, for requests that target single resources
    ///     an array of resource objects, an array of resource identifier objects, or an empty array([]), for requests that target resource collections
    /// 
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class JsonApiDocument
    {
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public JObject JsonApi => new JObject(new JProperty("version", "1.0"));

        [JsonConverter(typeof(JsonDocumentDataConverter))]
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<JsonApiResourceObject> Data { get; set; }
        
        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<JsonApiErrorObject> Errors { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiResourceObjectDictionary Included { get; set; }
    }

    
}
