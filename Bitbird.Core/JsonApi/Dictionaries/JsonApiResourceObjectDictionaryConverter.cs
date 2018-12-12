using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi.Dictionaries
{
    public class JsonApiResourceObjectDictionaryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(JsonApiResourceObjectDictionary);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var dictionary = existingValue as JsonApiResourceObjectDictionary;
            var jArray = JArray.Load(reader);
            foreach (var resourceJObject in jArray)
            {
                var resource = resourceJObject.ToObject<JsonApiResourceObject>(serializer);
                dictionary.AddResource(resource);
            }
            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dictionary = value as JsonApiResourceObjectDictionary;
            JArray jArray = new JArray();
            foreach (var resourceObject in dictionary.ResourceObjectDictionary.Values)
            {
                var o = JObject.FromObject(resourceObject, serializer);
                jArray.Add(o);
            }
            jArray.WriteTo(writer);
        }
    }
}
