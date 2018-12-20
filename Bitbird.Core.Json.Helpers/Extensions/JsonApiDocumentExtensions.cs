using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Bitbird.Core.Json.Extensions;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    public static class JsonApiDocumentExtensions
    {
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

        public static void IncludeRelation(this JsonApiDocument document, object data, JsonApiResource apiResource, string propertyName)
        {
            var relation = apiResource.Relationships.Where(r => string.Equals(r.PropertyName, propertyName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if(relation == null) { return; }
            document.IncludeRelation(data, relation);
        }

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
    }
}
