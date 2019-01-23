using Bitbird.Core.Json.JsonApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Bitbird.Core.Json.Extensions;
using Bitbird.Core.Json.Helpers.Base.Extensions;
using System.Collections;

namespace Bitbird.Core.Json.Helpers.ApiResource.Extensions
{
    internal class IncludePathNode
    {
        public string PropertyPath { get; set; }
        public ResourceRelationship IncludeApiResourceRelationship {get; set;}
        public IncludePathNode Child { get; set; }
    }

    public static class JsonApiCollectionDocumentExtensions
    {
        #region CreateDocumentFromApiResource

        public static JsonApiCollectionDocument CreateDocumentFromApiResource<T>(IEnumerable data, string baseUrl = null) where T : JsonApiResource
        {
            T apiResource = Activator.CreateInstance<T>();
            var document = new JsonApiCollectionDocument();
            document.FromApiResource(data, apiResource, baseUrl);
            return document;
        }

        #endregion

        #region FromApiResource

        public static void FromApiResource(this JsonApiCollectionDocument document, IEnumerable data, JsonApiResource apiResource, string baseUrl = null)
        {
            if (data == null) { return; }
            List<JsonApiResourceObject> resourceObjects = new List<JsonApiResourceObject>();
            foreach (var item in data)
            {
                var rObject = new JsonApiResourceObject();
                rObject.FromApiResource(item, apiResource, baseUrl);
                resourceObjects.Add(rObject);
            }
            document.Data = resourceObjects;
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                document.Links = new JsonApiLinksObject
                {
                    Self = new JsonApiLink
                    {
                        Href = $"{baseUrl}{apiResource.UrlPath}"
                    }
                };
            }
        }

        #endregion
        
        #region ToObjectCollection

        public static IEnumerable<T_Result> ToObjectCollection<T_Result, T_Resource>(this JsonApiCollectionDocument document) where T_Resource : JsonApiResource
        {
            var primaryResourceObjects = document.Data;
            if (primaryResourceObjects == null) throw new Exception("Json document contains no data.");
            return primaryResourceObjects.Select(r => r.ToObject<T_Result, T_Resource>()).ToList();
        }

        public static IEnumerable<T_Result> ToObjectCollection<T_Result, T_Resource>(this JsonApiCollectionDocument document, out Func<int, string, bool> foundAttributes) where T_Resource : JsonApiResource
        {
            foundAttributes = (idx, attrName) => (document.Data.ElementAt(idx)?.Attributes?.ContainsKey(attrName.ToJsonAttributeName())).Value;
            return document.ToObjectCollection<T_Result, T_Resource>();
        }

        internal static IEnumerable<T> Cast<T>(IEnumerable<JsonApiResourceObject> data, JsonApiResource apiResource)
        {
            return data.Select(r => (T)r.ToObject(apiResource, typeof(T))).ToList();
        }

        /// <summary>
        /// Extracts apiResource to an IEmumerable of type targetType.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="apiResource"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ToObjectCollection(this JsonApiCollectionDocument document, JsonApiResource apiResource, Type targetType)
        {
            if (targetType.IsNonStringEnumerable())
            {
                throw new Exception("Do not use a collection as target type!");
            }
            var primaryResourceObjects = document.Data;
            if (primaryResourceObjects == null) throw new Exception("Json document contains no data.");

            var method = typeof(JsonApiCollectionDocumentExtensions).GetMethod(nameof(JsonApiCollectionDocumentExtensions.Cast), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(targetType);
            return method.Invoke(null, new object[] { primaryResourceObjects, apiResource });
        }

        public static object ToObjectCollection(this JsonApiCollectionDocument document, JsonApiResource apiResource, Type targetType, out Func<int, string, bool> foundAttributes)
        {
            foundAttributes = (idx, attrName) => (document.Data.ElementAt(idx)?.Attributes?.ContainsKey(attrName.ToJsonAttributeName())).Value;
            return document.ToObjectCollection(apiResource, targetType);
        }

        #endregion

        #region GetIncludedResource

        public static T_Result GetIncludedResource<T_Result, T_ResultApiResource>(this JsonApiCollectionDocument document, object id) where T_ResultApiResource : JsonApiResource where T_Result : class
        {
            return (T_Result)document.GetIncludedResource(id, typeof(T_Result), Activator.CreateInstance<T_ResultApiResource>());
        }

        public static T_Result GetIncludedResource<T_Result>(this JsonApiCollectionDocument document, object id, JsonApiResource apiResource) where T_Result : class
        {
            return (T_Result)document.GetIncludedResource(id, typeof(T_Result), apiResource);
        }

        public static object GetIncludedResource(this JsonApiCollectionDocument document, object id, Type type, JsonApiResource apiResource)
        {
            return document.Included?.GetResource(id, apiResource)?.ToObject(apiResource, type);
        }

        #endregion

    }

    public static class JsonApiDocumentExtensions
    {

        #region CreateDocumentFromApiResource

        public static JsonApiDocument CreateDocumentFromApiResource<T>(object data, string baseUrl = null) where T : JsonApiResource
        {
            T apiResource = Activator.CreateInstance<T>();
            JsonApiDocument document = new JsonApiDocument();
            document.FromApiResource(data, apiResource, baseUrl);
            return document;
        }

        #endregion

        #region FromApiResource

        public static void FromApiResource(this JsonApiDocument document, object data, JsonApiResource apiResource, string baseUrl = null)
        {
            if(data == null) { return; }
            if(data is IEnumerable<object> enumCollection)
            {
                throw new Exception("data cannot be a collection");
            }
            else
            {
                var rObject = new JsonApiResourceObject();
                rObject.FromApiResource(data, apiResource, baseUrl);
                document.Data = rObject;
                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    document.Links = new JsonApiLinksObject
                    {
                        Self = new JsonApiLink
                        {
                            Href = $"{baseUrl}{apiResource.UrlPath}/{rObject.Id}"
                        }
                    };
                }
            }
        }

        #endregion

        #region IncludeRelation
        
        public static void IncludeRelation<T_Resource>(this JsonApiDocument document, object data, string path, string baseUrl = null) where T_Resource : JsonApiResource
        {
            document.IncludeRelation(Activator.CreateInstance<T_Resource>(), data, path, baseUrl);
        }

        public static void IncludeRelation(this JsonApiDocument document, JsonApiResource dataApiResource, object data, string path, string baseUrl = null)
        {
            // parse paths
            var subpaths = path.Split(new char[] { ',' });
            var collection = data as IEnumerable<object>;
            if (collection != null)
            {
                foreach (var item in collection)
                {
                    foreach (var includePath in subpaths)
                    {
                        // generate tree
                        var includePathTree = GenerateIncludeTree(dataApiResource, includePath);
                        // process tree
                        ProcessIncludeTree(document, includePathTree, item, baseUrl);
                    }
                }
            }
            else
            {
                foreach (var includePath in subpaths)
                {
                    // generate tree
                    var includePathTree = GenerateIncludeTree(dataApiResource, includePath);
                    // process tree
                    ProcessIncludeTree(document, includePathTree, data, baseUrl);
                }
            }
        }

        private static void ProcessIncludeTree(JsonApiDocument document, IncludePathNode includePathTree, object data, string baseUrl)
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
                jsonResourceObject.FromApiResource(value, relationship.RelatedResource, baseUrl);
                if (document.Included == null) { document.Included = new JsonApi.Dictionaries.JsonApiResourceObjectDictionary(); }
                document.Included.AddResource(jsonResourceObject);
                if(includePathTree.Child != null)
                {
                    // jump down the rabbit hole
                    ProcessIncludeTree(document, includePathTree.Child, value, baseUrl);
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
                        jsonResourceObject.FromApiResource(item, relationship.RelatedResource, baseUrl);
                        document.Included.AddResource(jsonResourceObject);
                        if (includePathTree.Child != null)
                        {
                            // jump down the rabbit hole
                            ProcessIncludeTree(document, includePathTree.Child, item, baseUrl);
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

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <typeparam name="T_Result">The Type of the exracted Data.</typeparam>
        /// <typeparam name="T_Resource">The Type of JsonApiResource used to extract the Data.</typeparam>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// ModelType m = jsonDocument.ToObject<ModelType, ModelTypeApiResource>();
        /// </code>
        /// </example>
        public static T_Result ToObject<T_Result, T_Resource>(this JsonApiDocument document) where T_Resource : JsonApiResource
        {
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            // extract primary data
            return primaryResourceObject.ToObject<T_Result, T_Resource>();
        }

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <typeparam name="T_Result">The Type of the exracted Data.</typeparam>
        /// <typeparam name="T_Resource">The Type of JsonApiResource used to extract the Data.</typeparam>
        /// <param name="foundAttributes">A function to determine which attributes were found in the JsonDocument's primary data</param>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// Func<string, bool> foundAttributes;
        /// ModelType m = jsonDocument.ToObject<ModelType, ModelTypeApiResource>(out foundAttributes);
        /// </code>
        /// </example>
        public static T_Result ToObject<T_Result, T_Resource>(this JsonApiDocument document, out Func<string, bool> foundAttributes) where T_Resource : JsonApiResource
        {
            var attrs = document.Data.Attributes;
            foundAttributes = (attrName) => attrs != null ? attrs.ContainsKey(attrName.ToJsonAttributeName()) : false;
            return document.ToObject<T_Result, T_Resource>();
        }

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <param name="apiResource">instance of JsonApiResource used to extract the Data.</param>
        /// <param name="targetType">The Type of the exracted Data.</param>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// var collection = jsonDocument.ToObject(Activator.CreateInstance<ModelTypeApiResource>(), typeof(IEnumerable<ModelType>));
        /// var singleItem = jsonDocument.ToObject(Activator.CreateInstance<ModelTypeApiResource>(), typeof(ModelType));
        /// </code>
        /// </example>
        public static object ToObject(this JsonApiDocument document, JsonApiResource apiResource, Type targetType)
        {
            if (targetType.IsNonStringEnumerable())
            {
                var innerType = targetType.GenericTypeArguments[0];
                return document.ToObjectCollection(apiResource, innerType);
            }
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            // extract primary data
            return primaryResourceObject.ToObject(apiResource, targetType);
        }

        /// <summary>
        /// Extract primary Data from the JsonApiDocument.
        /// </summary>
        /// <param name="apiResource">instance of JsonApiResource used to extract the Data.</param>
        /// <param name="targetType">The Type of the exracted Data.</param>
        /// <param name="foundAttributes">A function to determine which attributes were found in the JsonDocument's primary data</param>
        /// <returns>An instance containing the model data.</returns>
        /// <example>
        /// <code>
        /// Func<string, bool> foundAttributes;
        /// var collection = jsonDocument.ToObject(Activator.CreateInstance<ModelTypeApiResource>(), typeof(IEnumerable<ModelType>, out foundAttributes));
        /// var singleItem = jsonDocument.ToObject(Activator.CreateInstance<ModelTypeApiResource>(), typeof(ModelType), out foundAttributes);
        /// </code>
        /// </example>
        public static object ToObject(this JsonApiDocument document, JsonApiResource apiResource, Type targetType, out Func<string, bool> foundAttributes)
        {
            var attrs = document.Data.Attributes;
            foundAttributes = (attrName) => attrs != null ? attrs.ContainsKey(attrName.ToJsonAttributeName()) : false;
            return document.ToObject(apiResource, targetType);
        }

        #endregion

        #region ToObjectCollection

        public static IEnumerable<T_Result> ToObjectCollection<T_Result, T_Resource>(this JsonApiDocument document) where T_Resource : JsonApiResource
        {
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");
            return new List<T_Result> { primaryResourceObject.ToObject<T_Result, T_Resource>() };
        }
        
        public static IEnumerable<T_Result> ToObjectCollection<T_Result, T_Resource>(this JsonApiDocument document, out Func<string, bool> foundAttributes) where T_Resource : JsonApiResource
        {
            foundAttributes = (attrName) => (document.Data?.Attributes?.ContainsKey(attrName.ToJsonAttributeName())).Value;
            return document.ToObjectCollection<T_Result, T_Resource>();
        }

        internal static IEnumerable<T> Cast<T>(JsonApiResourceObject data, JsonApiResource apiResource)
        {
            return new List<T> { (T)data.ToObject(apiResource, typeof(T)) };
        }

        /// <summary>
        /// Extracts apiResource to an IEmumerable of type targetType.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="apiResource"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object ToObjectCollection(this JsonApiDocument document, JsonApiResource apiResource, Type targetType)
        {
            if (targetType.IsNonStringEnumerable())
            {
                throw new Exception("Do not use a collection as target type!");
            }
            var primaryResourceObject = document.Data;
            if (primaryResourceObject == null) throw new Exception("Json document contains no data.");

            var method = typeof(JsonApiDocumentExtensions).GetMethod(nameof(JsonApiDocumentExtensions.Cast), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(targetType);
            return method.Invoke(null, new object[] { primaryResourceObject, apiResource });
        }
        
        public static object ToObjectCollection(this JsonApiDocument document, JsonApiResource apiResource, Type targetType, out Func<string, bool> foundAttributes)
        {
            foundAttributes = (attrName) => (document.Data.Attributes?.ContainsKey(attrName.ToJsonAttributeName())).Value;
            return document.ToObjectCollection(apiResource, targetType);
        }

        #endregion

        #region GetIncludedResource

        public static T_Result GetIncludedResource<T_Result,T_ResultApiResource>(this JsonApiDocument document, object id) where T_ResultApiResource : JsonApiResource where T_Result : class
        {
            return (T_Result)document.GetIncludedResource(id, typeof(T_Result), Activator.CreateInstance<T_ResultApiResource>());
        }

        public static T_Result GetIncludedResource<T_Result>(this JsonApiDocument document, object id, JsonApiResource apiResource) where T_Result : class
        {
            return (T_Result)document.GetIncludedResource(id, typeof(T_Result), apiResource);
        }

        public static object GetIncludedResource(this JsonApiDocument document, object id, Type type,JsonApiResource apiResource)
        {
            return document.Included?.GetResource(id, apiResource)?.ToObject(apiResource, type);
        }

        #endregion
    }
}
