using Bitbird.Core.Json.JsonApi.Dictionaries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.JsonApi
{
    public class JsonApiCollectionDocument : IJsonApiDocument
    {
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
