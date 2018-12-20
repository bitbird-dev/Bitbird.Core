using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.Json.JsonApi
{
    public class JsonApiResourceIdentifierObjectConverter : JsonConverter<JsonApiResourceIdentifierObject>
    {
        public override JsonApiResourceIdentifierObject ReadJson(JsonReader reader, Type objectType, JsonApiResourceIdentifierObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JsonApiResourceIdentifierObject result = existingValue ?? new JsonApiResourceIdentifierObject();
            var jObject = JObject.Load(reader);
            result.Id = jObject.Property("id").Value.ToObject<string>();
            result.Type = jObject.Property("type").Value.ToObject<string>();
            result.Meta = jObject.Property("meta")?.Value?.ToObject<object>();
            return result;
        }

        public override void WriteJson(JsonWriter writer, JsonApiResourceIdentifierObject value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(value.Id);
            writer.WritePropertyName("type");
            writer.WriteValue(value.Id);
            if(value.Meta != null)
            {
                writer.WritePropertyName("meta");
                writer.WriteValue(value.Meta);
            }
            writer.WriteEndObject();
        }
    }

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
        [JsonProperty("id", NullValueHandling = NullValueHandling.Include)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Include)]
        public string Type { get; set; }

        [JsonProperty("meta", NullValueHandling = NullValueHandling.Ignore)]
        public object Meta { get; set; }

        public JsonApiResourceIdentifierObject() { }

        public JsonApiResourceIdentifierObject(string id, string type, object meta = null)
        {
            Id = id;
            Type = type;
            Meta = meta;
        }

        public IJsonApiDataModel ToObject(Type type)
        {
            IJsonApiDataModel result = null;

            try
            {
                result = Activator.CreateInstance(type) as IJsonApiDataModel;
                result.SetIdFromString(Id);
            }
            catch { }

            return result;
        }
    }
}
