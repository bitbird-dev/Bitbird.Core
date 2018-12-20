using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.JsonApi.Attributes;
using Bitbird.Core.Json.JsonApi.UrlBuilder;
using Bitbird.Core.Json.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Bitbird.Core.Json.JsonApi
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
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Include)]
        public string Type { get; set; }

        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, JsonApiRelationshipObjectBase> Relationships { get; set; }

        [JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
        public JsonApiLinksObject Links { get; set; }

        #region Constructor

        public JsonApiResourceObject() { }

        public JsonApiResourceObject(IJsonApiDataModel data, bool AutoProcessRelationships) : this(data, null, AutoProcessRelationships) { }
        
        public JsonApiResourceObject(IJsonApiDataModel data, Uri queryUri = null, bool AutoProcessRelationships = true)
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
        public T ToObject<T>(bool processRelations = true) where T : IJsonApiDataModel
        {
            return (T)ToObject(typeof(T), processRelations);
        }

        /// <summary>
        /// Converts the resource to a object.
        /// </summary>
        /// <typeparam name="t"></typeparam>
        /// <returns></returns>
        public IJsonApiDataModel ToObject(Type t, bool processRelations = true)
        {

            IJsonApiDataModel result = null;

            // try to create object from attributes
            try
            {
                if (Attributes == null) { result = Activator.CreateInstance(t) as IJsonApiDataModel; }
                else { result = JObject.FromObject(Attributes).ToObject(t) as IJsonApiDataModel; }
                result.SetIdFromString(Id);
            }
            catch { }
            if (!processRelations || result == null || Relationships == null || Relationships.Count < 1) { return result; }
            
            var idPropertyDict = t.GetProperties().Where(p => p.GetCustomAttribute<JsonApiRelationIdAttribute>() != null).ToDictionary(k=> k.GetCustomAttribute<JsonApiRelationIdAttribute>().PropertyName, v=> v);
            Dictionary<string, PropertyInfo> refPropertyDict = null;
            Dictionary<string, string> refKeyToName = null;
            {
                var refProperties = t.GetProperties().Where(p => typeof(IJsonApiDataModel).IsAssignableFrom(p.PropertyType) || typeof(IEnumerable<IJsonApiDataModel>).IsAssignableFrom(p.PropertyType));
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
                            var item = Activator.CreateInstance(innerType) as IJsonApiDataModel;
                            item.SetIdFromString(r?.Id);
                            collection.Add(item);
                        }
                        refInfo.SetValue(result, collection);
                    }
                    else
                    {
                        var relationConcrete = relation.Value as JsonApiToOneRelationshipObject;
                        var item = Activator.CreateInstance(refInfo.PropertyType) as IJsonApiDataModel;
                        item.SetIdFromString(relationConcrete.Data.Id);
                        refInfo.SetValue(result, item);
                    }
                }
            }

            return result;
        }

        #endregion
    }

    //public static class JsonApiResourceObjectExtensions
    //{
    //    public static void SetIdAndType(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string customIdPropertyName = null, string customTypeName = null)
    //    {
    //        resourceObject.Type = (string.IsNullOrWhiteSpace(customTypeName)) ? customTypeName : data.GetJsonApiClassName();
            
    //        if (string.IsNullOrWhiteSpace(customIdPropertyName))
    //        {
    //            resourceObject.Id = data.GetIdAsString();
    //        }
    //        else
    //        {
    //            var dataType = data.GetType();
    //            var propertyInfo = dataType.GetProperty(customIdPropertyName);
    //            var value = propertyInfo.GetValueFast(data);
    //            resourceObject.Id = (value==null)?null:JValue.FromObject(value)?.ToString();
    //        }
            
    //    }

    //    public static void AddAttribute(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string propertyName, string attributeName = null)
    //    {
    //        var dataType = data.GetType();
    //        var propertyInfo = dataType.GetProperty(propertyName);
    //        var key = string.IsNullOrWhiteSpace(attributeName) ? StringUtils.GetAttributeName(propertyInfo) : attributeName;
    //        var value = propertyInfo.GetValueFast(data);
    //        if (resourceObject.Attributes == null) { resourceObject.Attributes = new Dictionary<string, object>(); }
    //        resourceObject.Attributes.Add(key, value);
    //    }

    //    public static void AddToOneRelationship(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string propertyName, string relationshipName = null)
    //    {
    //        var dataType = data.GetType();
    //        var propertyInfo = dataType.GetProperty(propertyName);
    //        var key = string.IsNullOrWhiteSpace(relationshipName) ? StringUtils.GetRelationShipName(propertyInfo) : relationshipName;
    //        var value = propertyInfo.GetValueFast(data) as IJsonApiDataModel;
    //        if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
    //        JsonApiToOneRelationshipObject relation = new JsonApiToOneRelationshipObject();
    //        if(value != null)
    //        {
    //            relation.Data = new JsonApiResourceIdentifierObject
    //            {
    //                Id = value.GetIdAsString(),
    //                Type = value.GetJsonApiClassName()
    //            };
    //        }

    //        resourceObject.Relationships.Add(key, relation);
    //    }

    //    public static void AddToManyRelationship(this JsonApiResourceObject resourceObject, IJsonApiDataModel data, string propertyName, string relationshipName = null)
    //    {
    //        var dataType = data.GetType();
    //        var propertyInfo = dataType.GetProperty(propertyName);
    //        var key = string.IsNullOrWhiteSpace(relationshipName) ? StringUtils.GetRelationShipName(propertyInfo) : relationshipName;
    //        var value = propertyInfo.GetValueFast(data) as IEnumerable<IJsonApiDataModel>;
    //        if (resourceObject.Relationships == null) { resourceObject.Relationships = new Dictionary<string, JsonApiRelationshipObjectBase>(); }
    //        JsonApiToManyRelationshipObject relationshipCollection = new JsonApiToManyRelationshipObject();
    //        if (value != null)
    //        {
    //            foreach(var referencedData in value)
    //            {
    //                relationshipCollection.Data.Add( new JsonApiResourceIdentifierObject
    //                {
    //                    Id = referencedData.GetIdAsString(),
    //                    Type = referencedData.GetJsonApiClassName()
    //                });
    //            }
    //        }

    //        resourceObject.Relationships.Add(key, relationshipCollection);
    //    }
    //}


}
