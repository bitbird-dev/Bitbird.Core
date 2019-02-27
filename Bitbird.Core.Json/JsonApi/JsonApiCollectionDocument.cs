using Bitbird.Core.Json.JsonApi.Dictionaries;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bitbird.Core.Json.JsonApi
{
    public class JsonApiCollectionDocument : IJsonApiDocument
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "data")]
        public IEnumerable<JsonApiResourceObject> Data { get; set; }

        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "errors")]
        public IEnumerable<JsonApiErrorObject> Errors { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "meta")]
        public object Meta { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "links")]
        public JsonApiLinksObject Links { get; set; }

        [JsonProperty("included", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "included")]
        public JsonApiResourceObjectDictionary Included { get; set; }
    }
}
