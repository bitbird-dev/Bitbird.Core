using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
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

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Self { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Related { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Prev { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Next { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink First { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Last { get; set; }
    }
}
