using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.JsonDataModel.Converters;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    public static class JsonApiDocumentExtensions
    {
        #region CreateDocumentFromApiResource

        public static JsonApiDocument CreateDocumentFromApiResource<T>(object data) where T : JsonApiResource
        {
            T apiResource = Activator.CreateInstance<T>();
            JsonApiDocument document = null;
            document = new JsonApiDocument();
            document.FromApiResource(data, apiResource);
            return document;
        }

        #endregion

        #region FromApiResource

        public static void FromApiResource(this JsonApiDocument document, object data, JsonApiResource apiResource)
        {
            if(data == null) { return; }
            var collection = data as IEnumerable<object>;
            if(collection != null)
            {
                List<JsonApiResourceObject> resourceObjects = new List<JsonApiResourceObject>();
                foreach (var item in collection)
                {
                    var rObject = new JsonApiResourceObject();
                    rObject.FromApiResource(data, apiResource);
                    resourceObjects.Add(rObject);
                }
                document.Data = resourceObjects;
            }
            else
            {
                var rObject = new JsonApiResourceObject();
                rObject.FromApiResource(data, apiResource);
                document.Data = new List<JsonApiResourceObject> { rObject };
            }
            
        }

        #endregion

        #region IncludeRelation

        public static void IncludeRelation(this JsonApiDocument document, object data, JsonApiResource apiResource, string propertyName)
        {
            var relation = apiResource.Relationships.Where(r => string.Equals(r.PropertyName, propertyName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if(relation == null) { return; }
            document.IncludeRelation(data, relation);
        }

        public static void IncludeRelation<T_Resource>(this JsonApiDocument document, object data, string propertyName) where T_Resource : JsonApiResource
        {
            T_Resource apiResource = Activator.CreateInstance<T_Resource>();
            var relation = apiResource.Relationships.Where(r => string.Equals(r.PropertyName, propertyName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (relation == null) { return; }
            document.IncludeRelation(data, relation);
        }

        #endregion

        #region IncludeRelation

        internal static void IncludeRelation(this JsonApiDocument document, object data, ResourceRelationship relationship)
        {
            var dataType = data.GetType();
            var propertyInfo = dataType.GetProperty(relationship.PropertyName);
            if(propertyInfo == null) { throw new Exception($"Property {relationship.PropertyName} does not exist for type {dataType.Name}."); }
            var value = propertyInfo.GetValueFast(data);
            if (value == null) { return; }
            if(relationship.Kind == RelationshipKind.BelongsTo)
            {
                var jsonResourceObject = new JsonApiResourceObject();
                jsonResourceObject.FromApiResource(value, relationship.RelatedResource);
                if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                document.Included.AddResource(jsonResourceObject);
            }
            else
            {
                var collection = value as IEnumerable<object>;
                if(collection != null && collection.Count() > 0)
                {
                    if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                    foreach (var item in collection)
                    {
                        var jsonResourceObject = new JsonApiResourceObject();
                        jsonResourceObject.FromApiResource(item, relationship.RelatedResource);
                        document.Included.AddResource(jsonResourceObject);
                    }
                }
                
            }
            
        }

        #endregion

        #region ToObject

        public static T_Result ToObject<T_Result, T_Resource>(this JsonApiDocument document) where T_Resource : JsonApiResource
        {
            var primaryResourceObject = document.Data.FirstOrDefault();
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            // extract primary data
            return primaryResourceObject.ToObject<T_Result, T_Resource>();
        }

        public static T_Result ToObject<T_Result, T_Resource>(this JsonApiDocument document, out Func<string, bool> foundAttributes) where T_Resource : JsonApiResource
        {
            var attrs = document.Data.FirstOrDefault()?.Attributes;
            foundAttributes = (attrName) => attrs != null ? attrs.ContainsKey(attrName.ToLowerInvariant()) : false;
            return document.ToObject<T_Result, T_Resource>();
        }

        #endregion

        #region ToObjectCollection

        public static IEnumerable<T_Result> ToObjectCollection<T_Result, T_Resource>(this JsonApiDocument document) where T_Resource : JsonApiResource
        {
            var primaryResourceObjects = document.Data;
            if (primaryResourceObjects == null) throw new Exception("Json document contains no data.");
            return primaryResourceObjects.Select(r => r.ToObject<T_Result, T_Resource>());
        }

        public static IEnumerable<T_Result> ToObjectCollection<T_Result, T_Resource>(this JsonApiDocument document, out Func<int, string, bool> foundAttributes) where T_Resource : JsonApiResource
        {
            foundAttributes = (idx, attrName) => (document.Data.ElementAt(idx)?.Attributes?.ContainsKey(attrName.ToLowerInvariant())).Value;
            return document.ToObjectCollection<T_Result, T_Resource>();
        }

        #endregion

        public static JsonApiResourceObject GetResource(this JsonApi.Dictionaries.JsonApiResourceObjectDictionary resourceDictionary, object id, Type type)
        {
            return resourceDictionary.GetResource(BtbrdCoreIdConverters.ConvertToString(id), type.GetJsonApiClassName());
        }
    }
}
