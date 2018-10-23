using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public abstract class JsonApiBaseModel
    {
        [JsonIgnore, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
    }
}
