using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

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
        public JsonApiLink Self { get; set; }
        
        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Related { get; set; }
        
        [JsonProperty("prev", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Prev { get; set; }
        
        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Next { get; set; }
        
        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink First { get; set; }
        
        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Last { get; set; }
    }
}
