using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Bitbird.Core.Json.JsonApi
{

    /// <summary>
    /// Where specified, a links member can be used to represent links. 
    /// The value of each links member MUST be an object (a “links object”).
    /// </summary>
    public sealed class JsonApiLinksObject
    {
        public JsonApiLinksObject(JsonApiLink self = null, JsonApiLink related = null)
        {
            Self = self;
            Related = related;
        }
        
        [JsonProperty("self",NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "self")]
        public JsonApiLink Self { get; set; }
        
        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "related")]
        public JsonApiLink Related { get; set; }
        
        [JsonProperty("prev", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "prev")]
        public JsonApiLink Prev { get; set; }
        
        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "next")]
        public JsonApiLink Next { get; set; }
        
        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "first")]
        public JsonApiLink First { get; set; }
        
        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "last")]
        public JsonApiLink Last { get; set; }
    }
}
