using Bitbird.Core.Extensions;
using Bitbird.Core.JsonApi.Attributes;
using Bitbird.Core.JsonApi.UrlBuilder;
using Bitbird.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.JsonApi
{
    public class JsonApiResourceObjectConverter : JsonConverter<JsonApiResourceObject>
    {
        public override JsonApiResourceObject ReadJson(JsonReader reader, Type objectType, JsonApiResourceObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, JsonApiResourceObject value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class JsonApiResourceObject
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        public string Type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JsonApiRelationshipObjectBase> Relationships { get; set; } = new Dictionary<string, JsonApiRelationshipObjectBase>();

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        #region Constructor

        public JsonApiResourceObject() { }

        public JsonApiResourceObject(JsonApiBaseModel data, bool AutoProcessRelationships) : this(data, null, AutoProcessRelationships) { }
        
        public JsonApiResourceObject(JsonApiBaseModel data, Uri queryUri = null, bool AutoProcessRelationships = true)
        {
            var builder = new JsonApiResourceBuilder();
            var res = builder.Build(data, AutoProcessRelationships);
            this.Id = res.Id;
            this.Type = res.Type;
            this.Attributes = res.Attributes;
            this.Relationships = res.Relationships;

            // set url
            if (queryUri != null)
            {
                Links = new JsonApiLinksObject { Self = new JsonApiLink(queryUri.GetFullHost() + (new DefaultUrlPathBuilder()).BuildCanonicalPath(data)) };
            }
        }

        #endregion

        #region ToObject

        /// <summary>
        /// Converts the resource to a object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T ToObject<T>(bool processRelations = true) where T : JsonApiBaseModel
        {
            return ToObject(typeof(T), processRelations) as T;
        }

        /// <summary>
        /// Converts the resource to a object.
        /// </summary>
        /// <typeparam name="t"></typeparam>
        /// <returns></returns>
        public JsonApiBaseModel ToObject(Type t, bool processRelations = true)
        {

            JsonApiBaseModel result = null;

            // try to create object from attributes
            try
            {
                if (Attributes == null) { result = Activator.CreateInstance(t) as JsonApiBaseModel; }
                else { result = JObject.FromObject(Attributes).ToObject(t) as JsonApiBaseModel; }
                result.Id = Id;
            }
            catch { }
            if (!processRelations || result == null || Relationships == null || Relationships.Count < 1) { return result; }
            
            var idPropertyDict = t.GetProperties().Where(p => p.GetCustomAttribute<JsonApiRelationIdAttribute>() != null).ToDictionary(k=> k.GetCustomAttribute<JsonApiRelationIdAttribute>().PropertyName, v=> v);
            Dictionary<string, PropertyInfo> refPropertyDict = null;
            Dictionary<string, string> refKeyToName = null;
            {
                var refProperties = t.GetProperties().Where(p => typeof(JsonApiBaseModel).IsAssignableFrom(p.PropertyType) || typeof(IEnumerable<JsonApiBaseModel>).IsAssignableFrom(p.PropertyType));
                refPropertyDict = refProperties.ToDictionary(k => StringUtils.GetRelationShipName(k), v => v);
                refKeyToName = refProperties.ToDictionary(k => StringUtils.GetRelationShipName(k), v => v.Name);
            }
                
            foreach (var relation in Relationships)
            {
                string propname;
                if(refKeyToName.TryGetValue(relation.Key , out propname))
                {
                    PropertyInfo propertyInfo = null;
                    if(idPropertyDict.TryGetValue(propname, out propertyInfo))
                    {
                        if(propertyInfo.PropertyType is IEnumerable)
                        {
                            var relationConcrete = relation.Value as JsonApiToManyRelationshipObject;
                            if (relationConcrete.Data != null)
                            {
                                propertyInfo.SetValue(result, relationConcrete.Data.Select(r => StringUtils.ConvertId(r.Id, propertyInfo.PropertyType)));
                            }
                        }
                        else
                        {
                            var relationConcrete = relation.Value as JsonApiToOneRelationshipObject;
                            if(relationConcrete.Data != null)
                            { 
                                propertyInfo.SetValue(result, StringUtils.ConvertId(relationConcrete.Data.Id, propertyInfo.PropertyType));
                            }
                        }
                    }
                }

                PropertyInfo refInfo;
                if(refPropertyDict.TryGetValue(relation.Key, out refInfo))
                { 
                    if(refInfo.PropertyType.IsNonStringEnumerable())
                    {
                        var innerType = refInfo.PropertyType.GenericTypeArguments[0];
                        var constructedListType = typeof(List<>).MakeGenericType(innerType);
                        var collection = Activator.CreateInstance(constructedListType) as IList;
                        var relationConcrete = relation.Value as JsonApiToManyRelationshipObject;
                        foreach (var r in relationConcrete.Data)
                        {
                            var item = Activator.CreateInstance(innerType) as JsonApiBaseModel;
                            item.Id = r?.Id;
                            collection.Add(item);
                        }
                        refInfo.SetValue(result, collection);
                    }
                    else
                    {
                        var relationConcrete = relation.Value as JsonApiToOneRelationshipObject;
                        var item = Activator.CreateInstance(refInfo.PropertyType) as JsonApiBaseModel;
                        item.Id = relationConcrete.Data.Id;
                        refInfo.SetValue(result, item);
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
