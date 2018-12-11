using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    [JsonConverter(typeof(JsonApiLinkConverter))]
    public sealed class JsonApiLink
    {
        public JsonApiLink(string url, object metadata = null)
        {
            Href = url;
            Meta = metadata;
        }

        public JsonApiLink()
        {

        }

        public string Href { get; set; }
        
        public object Meta { get; set; }
    }

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
                return new JsonApiLink { Href = JValue.Load(reader).Value<string>() };
            }
            if(reader.TokenType == JsonToken.StartObject)
            {
                JObject linkJObject = JObject.Load(reader);
                var href = linkJObject.Property("href")?.Value?.ToObject<string>();
                if (string.IsNullOrWhiteSpace(href)) { throw new JsonException("invalid Href"); }
                var meta = linkJObject.Property("meta")?.Value?.ToObject<object>();
                return new JsonApiLink { Href = href , Meta = meta};
            }
            throw new JsonSerializationException("invalid JsonApiLink");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var link = value as JsonApiLink;
            if (string.IsNullOrWhiteSpace(link.Href))
            {
                throw new JsonException("invalid Href");
            }
            if (link.Meta == null)
            {
                writer.WriteValue(link.Href);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName("href");
                writer.WriteValue(link.Href);
                writer.WritePropertyName("meta");
                writer.WriteValue(link.Meta);
                writer.WriteEndObject();
            }
        }
    }

}
