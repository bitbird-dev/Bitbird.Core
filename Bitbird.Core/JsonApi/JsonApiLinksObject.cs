using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public class JsonApiLinkConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonApiLink).IsAssignableFrom(objectType) || typeof(string).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                var res = serializer.Deserialize<string>(reader);
                if (res != null)
                {
                    return new JsonApiLink(res);
                }
            }
            return serializer.Deserialize<JsonApiLink>(reader); ;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var link = value as JsonApiLink;
            if (link != null)
            {
                if (link.Meta == null) { serializer.Serialize(writer, link.Href); }
                else
                {
                    //var jo = new JObject(new JProperty("href", link.Href), new JProperty("meta", link.Meta));
                    serializer.Serialize(writer, link);
                }
            }
        }
    }

    /// <summary>
    /// Where specified, a links member can be used to represent links. 
    /// The value of each links member MUST be an object (a “links object”).
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public sealed class JsonApiLinksObject
    {
        public JsonApiLinksObject(JsonApiLink self = null, JsonApiLink related = null)
        {
            Self = self;
            Related = related;
        }

        [JsonConverter(typeof(JsonApiLinkConverter))]
        [JsonProperty("self",NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Self { get; set; }

        [JsonConverter(typeof(JsonApiLinkConverter))]
        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Related { get; set; }

        [JsonConverter(typeof(JsonApiLinkConverter))]
        [JsonProperty("prev", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Prev { get; set; }

        [JsonConverter(typeof(JsonApiLinkConverter))]
        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Next { get; set; }

        [JsonConverter(typeof(JsonApiLinkConverter))]
        [JsonProperty("first", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink First { get; set; }

        [JsonConverter(typeof(JsonApiLinkConverter))]
        [JsonProperty("last", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Last { get; set; }
    }
}
