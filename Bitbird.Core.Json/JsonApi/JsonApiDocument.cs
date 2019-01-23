using Bitbird.Core.Json.JsonApi.Dictionaries;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Bitbird.Core.Json.JsonApi
{
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
        
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiResourceObject Data { get; set; }
        
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
