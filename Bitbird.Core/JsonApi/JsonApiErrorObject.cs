using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.JsonApi
{
    public class JsonApiErrorLinksObject
    {
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink About { get; set; }
    }

    public class JsonApiErrorSource
    {
        /// <summary>
        /// a JSON Pointer [RFC6901] to the associated entity in the request document [e.g. "/data" for a primary data object, or "/data/attributes/title" for a specific attribute].
        /// </summary>
        [JsonProperty("pointer")]
        public string Pointer { get; set; }

        /// <summary>
        /// a string indicating which URI query parameter caused the error.
        /// </summary>
        [JsonProperty("parameter")]
        public string Parameter { get; set; }
    }

    public class JsonApiErrorObject
    {
        /// <summary>
        /// a unique identifier for this particular occurrence of the problem.
        /// </summary>
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        /// <summary>
        /// a links object containing the following members:
        /// </summary>
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiErrorLinksObject Links { get; set; }

        /// <summary>
        /// the HTTP status code applicable to this problem, expressed as a string value.
        /// </summary>
        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        /// <summary>
        /// an application-specific error code, expressed as a string value.
        /// </summary>
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        /// <summary>
        /// a short, human-readable summary of the problem that SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.
        /// </summary>
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// a human-readable explanation specific to this occurrence of the problem. Like title, this field’s value can be localized.
        /// </summary>
        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; set; }

        /// <summary>
        /// an object containing references to the source of the error, optionally including any of the following members:
        /// </summary>
        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiErrorSource Source { get; set; }
        
        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }
    }
}
