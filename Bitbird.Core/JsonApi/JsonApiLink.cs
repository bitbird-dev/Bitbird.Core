using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    /// <summary>
    /// Where specified, a links member can be used to represent links. 
    /// The value of each links member MUST be an object (a “links object”).
    /// 
    /// Each member of a links object is a “link”. A link MUST be represented as either:
    /// 
    /// a string containing the link’s URL.
    /// an object (“link object”) which can contain the following members:
    ///     href: a string containing the link’s URL.
    ///     meta: a meta object containing non-standard meta-information about the link.
    /// 
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class JsonApiLink
    {
        public JsonApiLink(string url, object metadata = null)
        {
            Href = url;
            Meta = metadata;
        }

        public string Href { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; }
    }
}
