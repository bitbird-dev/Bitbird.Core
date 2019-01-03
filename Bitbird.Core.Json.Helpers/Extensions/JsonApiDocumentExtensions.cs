using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.Base.Extensions;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    internal class IncludePathNode
    {
        public string PropertyPath { get; set; }
        public ResourceRelationship IncludeApiResourceRelationship {get; set;}
        public IncludePathNode Child { get; set; }
    }

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
                    rObject.FromApiResource(item, apiResource);
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
        
        public static void IncludeRelation<T_Resource>(this JsonApiDocument document, object data, string path) where T_Resource : JsonApiResource
        {
            // parse paths
            var subpaths = path.Split(new char[] { ',' });
            foreach(var includePath in subpaths)
            {
                // generate tree
                var includePathTree = GenerateIncludeTree(Activator.CreateInstance<T_Resource>(), includePath);
                // process tree
                ProcessIncludeTree(document, includePathTree, data);
            }
        }

        public static void IncludeRelation(this JsonApiDocument document, JsonApiResource dataApiResource, object data, string path)
        {
            // parse paths
            var subpaths = path.Split(new char[] { ',' });
            foreach (var includePath in subpaths)
            {
                // generate tree
                var includePathTree = GenerateIncludeTree(dataApiResource, includePath);
                // process tree
                ProcessIncludeTree(document, includePathTree, data);
            }
        }

        private static void ProcessIncludeTree(JsonApiDocument document, IncludePathNode includePathTree, object data)
        {
            var relationship = includePathTree.IncludeApiResourceRelationship;
            var dataType = data.GetType();
            var propertyInfo = dataType.GetProperty(relationship.PropertyName);
            if (propertyInfo == null) { throw new Exception($"Property {relationship.PropertyName} does not exist for type {dataType.Name}."); }
            var value = propertyInfo.GetValueFast(data);
            if (value == null) { return; }
            if (relationship.Kind == RelationshipKind.BelongsTo)
            {
                var jsonResourceObject = new JsonApiResourceObject();
                jsonResourceObject.FromApiResource(value, relationship.RelatedResource);
                if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                document.Included.AddResource(jsonResourceObject);
                if(includePathTree.Child != null)
                {
                    // jump down the rabbit hole
                    ProcessIncludeTree(document, includePathTree.Child, value);
                }
                
            }
            else
            {
                var collection = value as IEnumerable<object>;
                if (collection != null && collection.Count() > 0)
                {
                    if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                    foreach (var item in collection)
                    {
                        var jsonResourceObject = new JsonApiResourceObject();
                        jsonResourceObject.FromApiResource(item, relationship.RelatedResource);
                        document.Included.AddResource(jsonResourceObject);
                        if (includePathTree.Child != null)
                        {
                            // jump down the rabbit hole
                            ProcessIncludeTree(document, includePathTree.Child, item);
                        }
                    }
                }

            }

        }

        internal static IncludePathNode GenerateIncludeTree(JsonApiResource apiResource, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return null; }
            var subpaths = path.Split(new char[] { '.' }, 2);

            var relationshipName = subpaths[0].ToJsonRelationshipName();
            var relationshipResource = apiResource.Relationships.Where(x => x.Name == relationshipName).FirstOrDefault();
            if(relationshipResource == null) { throw new Exception($"Cannot include resource {path}: Path does not exist."); }
            var resultNode = new IncludePathNode
            {
                PropertyPath = relationshipName,
                IncludeApiResourceRelationship = relationshipResource,
            };
            resultNode.Child = (subpaths.Count() > 1) ? GenerateIncludeTree(relationshipResource.RelatedResource, subpaths[1]) : null;
            return resultNode;
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
            foundAttributes = (attrName) => attrs != null ? attrs.ContainsKey(attrName.ToJsonAttributeName()) : false;
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
            foundAttributes = (idx, attrName) => (document.Data.ElementAt(idx)?.Attributes?.ContainsKey(attrName.ToJsonAttributeName())).Value;
            return document.ToObjectCollection<T_Result, T_Resource>();
        }

        #endregion

        public static T_Result GetIncludedResource<T_Result,T_ResultApiResource>(this JsonApiDocument document, object id) where T_ResultApiResource : JsonApiResource where T_Result : class
        {
            return document.Included?.GetResource(id, typeof(T_Result))?.ToObject<T_Result, T_ResultApiResource>();
        }
    }
}
