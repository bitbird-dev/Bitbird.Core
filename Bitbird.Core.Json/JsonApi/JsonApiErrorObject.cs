using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Bitbird.Core.Json.JsonApi
{
    public class JsonApiErrorLinksObject
    {
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "about")]
        public JsonApiLink About { get; set; }
    }

    public class JsonApiErrorSource
    {
        /// <summary>
        /// a JSON Pointer [RFC6901] to the associated entity in the request document [e.g. "/data" for a primary data object, or "/data/attributes/title" for a specific attribute].
        /// </summary>
        [JsonProperty("pointer")]
        [DataMember(Name = "pointer")]
        public string Pointer { get; set; }

        /// <summary>
        /// a string indicating which URI query parameter caused the error.
        /// </summary>
        [JsonProperty("parameter")]
        [DataMember(Name = "parameter")]
        public string Parameter { get; set; }
    }

    public class JsonApiErrorObject
    {
        /// <summary>
        /// a unique identifier for this particular occurrence of the problem.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// a links object containing the following members:
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "links")]
        public JsonApiErrorLinksObject Links { get; set; }

        /// <summary>
        /// the HTTP status code applicable to this problem, expressed as a string value.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "status")]
        public string Status { get; set; }

        /// <summary>
        /// an application-specific error code, expressed as a string value.
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "code")]
        public string Code { get; set; }

        /// <summary>
        /// a short, human-readable summary of the problem that SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.
        /// </summary>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// a human-readable explanation specific to this occurrence of the problem. Like title, this field’s value can be localized.
        /// </summary>
        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "detail")]
        public string Detail { get; set; }

        /// <summary>
        /// an object containing references to the source of the error, optionally including any of the following members:
        /// </summary>
        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "source")]
        public JsonApiErrorSource Source { get; set; }
        
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        [DataMember(Name = "meta")]
        public object Meta { get; set; }
    }
}
