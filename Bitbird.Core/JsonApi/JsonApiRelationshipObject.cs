using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{

    public class JsonApiRelationshipObjectConverter : JsonConverter<JsonApiRelationshipObjectBase>
    {
        

        public override JsonApiRelationshipObjectBase ReadJson(JsonReader reader, Type objectType, JsonApiRelationshipObjectBase existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if(reader.TokenType != JsonToken.StartObject) { throw new JsonSerializationException("Invalid JsonApiRelationshipObject."); }
            JObject linkJObject = JObject.Load(reader);
            var links = linkJObject.Property("links")?.Value?.ToObject<JsonApiRelationshipLinksObject>();
            var meta = linkJObject.Property("meta")?.Value?.ToObject<JsonApiRelationshipLinksObject>();
            var dataToken = linkJObject.Property("data")?.Value;
            if(dataToken == null)
            {
                return new JsonApiToOneRelationshipObject { Links = links, Meta = meta };
            }
            else if (dataToken.Type == JTokenType.Array)
            {
                var data = dataToken.ToObject<List<JsonApiResourceIdentifierObject>>(serializer);
                return new JsonApiToManyRelationshipObject { Data = data, Links = links, Meta = meta };
            }
            else
            {
                var data = dataToken.ToObject<JsonApiResourceIdentifierObject>(serializer);
                return new JsonApiToOneRelationshipObject { Data = data, Links = links, Meta = meta };
            }
            throw new JsonSerializationException("Invalid JsonApiRelationshipObject.");
        }

        public void WriteToOneRelationship(JsonWriter writer, JsonApiToOneRelationshipObject value, JsonSerializer serializer)
        {
            
            if(value.Data == null)
            {
                //null for empty to-one relationships.
                writer.WriteNull();
            }
            else
            {
                //a single resource identifier object for non - empty to - one relationships.
                serializer.Serialize(writer, value.Data);
            }
        }

        public void WriteToManyRelationship(JsonWriter writer, JsonApiToManyRelationshipObject value, JsonSerializer serializer)
        {
            
            if (value.Data == null || value.Data.Count < 1)
            {
                //an empty array([]) for empty to-many relationships.
                writer.WriteStartArray();
                writer.WriteEndArray();
            }
            else
            {
                //an array of resource identifier objects for non - empty to - many relationships.
                serializer.Serialize(writer, value.Data);
            }
        }

        public override void WriteJson(JsonWriter writer, JsonApiRelationshipObjectBase value, JsonSerializer serializer)
        {
            if(value == null) { return; }
            // write "{"
            writer.WriteStartObject();
            // write "data:"
            writer.WritePropertyName("data");
            if(value.GetType() == typeof(JsonApiToOneRelationshipObject))
            {
                WriteToOneRelationship(writer, value as JsonApiToOneRelationshipObject, serializer);
            }
            else
            {
                WriteToManyRelationship(writer, value as JsonApiToManyRelationshipObject, serializer);

            }
            if(value.Links != null)
            {
                // write "links:"
                writer.WritePropertyName("links");
                serializer.Serialize(writer, value.Links);
            }
            if (value.Meta != null)
            {
                // write "meta:"
                writer.WritePropertyName("meta");
                serializer.Serialize(writer, value.Meta);
            }
            // write "}"
            writer.WriteEndObject();
        }
    }

    public class JsonApiRelationshipLinksObject
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Self { get; set; }

        [JsonProperty("related", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLink Related { get; set; }
    }

    [JsonConverter(typeof(JsonApiRelationshipObjectConverter))]
    public abstract class JsonApiRelationshipObjectBase
    {
        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiRelationshipLinksObject Links { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }
    }

    public class JsonApiToOneRelationshipObject : JsonApiRelationshipObjectBase
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Include)]
        public JsonApiResourceIdentifierObject Data { get; set; }
    }

    public class JsonApiToManyRelationshipObject : JsonApiRelationshipObjectBase
    {
        [JsonProperty("data")]
        public List<JsonApiResourceIdentifierObject> Data { get; set; } = new List<JsonApiResourceIdentifierObject>();
    }
}
