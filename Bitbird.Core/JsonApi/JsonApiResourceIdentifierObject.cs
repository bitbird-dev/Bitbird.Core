using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    /// <summary>
    /// A “resource identifier object” is an object that identifies an individual resource.
    /// 
    /// A “resource identifier object” MUST contain type and id members.
    /// 
    /// A “resource identifier object” MAY also include a meta member, 
    /// whose value is a meta object that contains non-standard meta-information.
    /// </summary>
    public class JsonApiResourceIdentifierObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Id { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string Type { get; private set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; private set; }

        public JsonApiResourceIdentifierObject(string id, string type, object meta = null)
        {
            Id = id;
            Type = type;
            Meta = meta;
        }
    }
}
