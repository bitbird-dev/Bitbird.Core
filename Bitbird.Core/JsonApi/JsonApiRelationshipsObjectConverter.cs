using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using Bitbird.Core.Utils;
using System.Collections.Generic;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public class JsonApiRelationshipsObjectConverter : JsonConverter
    {

        protected JsonApiRelationshipBase Create(Type objectType, JObject jObject, IContractResolver resolver)
        {
            if (FieldExists(jObject, StringUtils.ResolvePropertyName(resolver, nameof(JsonApiToOneRelationship.Data)), JTokenType.Object))
                return new JsonApiToOneRelationship();

            if (FieldExists(jObject, StringUtils.ResolvePropertyName(resolver, nameof(JsonApiToManyRelationship.Data)), JTokenType.Array))
                return new JsonApiToManyRelationship();

            if (FieldExists(jObject, StringUtils.ResolvePropertyName(resolver, nameof(JsonApiToOneRelationship.Links)), JTokenType.Object))
                return new JsonApiToOneRelationship();

            throw new InvalidOperationException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(JsonApiRelationshipBase).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => false;

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            if (FieldExists(jObject, StringUtils.ResolvePropertyName(serializer.ContractResolver, "data"), JTokenType.Null)) return null;

            JsonApiRelationshipBase target = Create(objectType, jObject, serializer.ContractResolver);
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        protected static bool FieldExists(
            JObject jObject,
            string name,
            JTokenType type)
        {
            JToken token;
            return jObject.TryGetValue(name, out token) && token.Type == type;
        }
    }
}
